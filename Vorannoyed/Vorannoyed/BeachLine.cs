using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Vorannoyed
{
    internal class BeachLine
    {
        private const float Epsilon = 0.0001f;

        private int capacity = 10;
        private BeachLineItem[] beachTree;
        private List<VEdge> edges;
        public List<VArc> arcs;

        internal BeachLine()
        {
            beachTree = new BeachLineItem[capacity];
            arcs = new List<VArc>();
            edges = new List<VEdge>();
        }

        internal void PrintBeachTree()
        {
            Console.WriteLine("Beach tree:");

            if (!HasBeachTreeNode(0))
            {
                Console.WriteLine("  <empty>");
                return;
            }

            BeachTreeDisplay display = BuildBeachTreeDisplay(0);
            foreach (string line in display.Lines)
            {
                Console.WriteLine(line.TrimEnd());
            }
        }

        internal void HandleVertexEvent(Vector2 eventLocation, List<VHalfEdge> halfEdges, HalfEdgeTracker halfEdgeTracker, ref Dictionary<VEvent, VEventInfo> events, ref VTile[] tiles, int curTileIndex, ref PriorityQueue priorityQueue)
        {
            //locate the existing arc (if any) that is above the new site
            int arcIndex = getExistingArc(beachTree, eventLocation);
            //break the arc by replacing the leaf node with a sub tree representing
            //the new arc and its break points
            tiles[curTileIndex] = new VTile(eventLocation);
            if (arcIndex == -1)
            {
                //beachTree[0] = new VArc(eventLocation);

                beachTree[0] = new BeachLineItem()
                {
                    IsEdge = false,
                    ArcIndex = arcs.Count,
                };
                arcs.Add(new VArc(eventLocation, tiles[curTileIndex]));
            }
            else
            {
                //Corresponding leaf replaced by a new sub-tree
                removeRelatedCircleEvent(arcIndex, eventLocation, ref events);
                VArc newArc = new VArc(eventLocation, tiles[curTileIndex]);
                VArc arc = arcs[beachTree[arcIndex].ArcIndex];


                //Add two half-edge records in the doubly linked list
                VHalfEdge halfEdge = new VHalfEdge()//pm pl
                {
                    Tile = newArc.Tile,
                };
                halfEdges.Add(halfEdge);
                VHalfEdge twinHalfEdge = new VHalfEdge()//pl pm
                {
                    Tile = arc.Tile,
                };
                //edges[edges.Count - 1].HalfEdge = halfEdge;
                //edges[edges.Count - 2].HalfEdge = twinHalfEdge;
                halfEdges.Add(twinHalfEdge);
                halfEdgeTracker.AddPair(halfEdge, twinHalfEdge);
                newArc.Tile.Neighbors.AddLast(arc.Tile);
                arc.Tile.Neighbors.AddLast(newArc.Tile);
                newArc.Tile.Edges.AddLast(halfEdge);//halfedge belongs to the newArc created
                arc.Tile.Edges.AddLast(twinHalfEdge);//created twinedge belongs to the split collided arc


                int leftChildIndex = getLeftChildIndex(arcIndex);
                beachTree[leftChildIndex] = new BeachLineItem()
                {
                    IsEdge = false,
                    ArcIndex = beachTree[arcIndex].ArcIndex,
                }; //arc;

                int rChild = getRightChildIndex(arcIndex);
                ensureExtraCapacity(getLeftChildIndex(rChild));
                beachTree[getLeftChildIndex(rChild)] = new BeachLineItem()
                {
                    IsEdge = false,
                    ArcIndex = arcs.Count,
                }; //newArc;

                arcs.Add(newArc);
                ensureExtraCapacity(getRightChildIndex(rChild));
                beachTree[getRightChildIndex(rChild)] = new BeachLineItem()
                {
                    IsEdge = false,
                    ArcIndex = arcs.Count,
                }; //arc;
                arcs.Add(new VArc(arc.Focus, arc.Tile));
                beachTree[rChild] = new BeachLineItem()
                {
                    IsEdge = true,
                    EdgeIndex = edges.Count,
                }; 
                edges.Add(new VEdge(beachTree[getLeftChildIndex(rChild)].ArcIndex, beachTree[getRightChildIndex(rChild)].ArcIndex, twinHalfEdge));
                //edges[edges.Count - 1].HalfEdge = halfEdges.Count;
                beachTree[arcIndex] = new BeachLineItem()
                {
                    IsEdge = true,
                    EdgeIndex = edges.Count,
                };
                edges.Add(new VEdge(beachTree[leftChildIndex].ArcIndex, beachTree[getLeftChildIndex(rChild)].ArcIndex, halfEdge));
                //edges[edges.Count - 1].HalfEdge = halfEdges.Count + 1;

                int edgeAncestorIndex = getFirstRightParentIndex(arcIndex);
                if (edgeAncestorIndex >= 0)
                {
                    int rightEdgeIndex = beachTree[edgeAncestorIndex].EdgeIndex;
                    edges[rightEdgeIndex].LeftArcIndex = arcs.Count - 1;
                }

                //Check for potential circle events, add them to event queue if they exist
                if (!hasParent(arcIndex))
                {
                    //there is no possible circle event,
                    return;
                }
                //int parentIndex = getParentIndex(arcIndex);//<-- get first right parent index //int rightParentIndex = getRightParentIndex(arcIndex);

                VEdge leftCreatedHalfEdge = edges[beachTree[arcIndex].EdgeIndex];
                int leftParentIndex = getFirstLeftParentIndex(arcIndex);
                //if(leftParentIndex == -1) -> then there is no need to check for edges on the left

                if (leftParentIndex >= 0)
                {
                    VEdge leftParentEdge = edges[beachTree[leftParentIndex].EdgeIndex];
                    Vector2 intercept = getRayIntercept(leftCreatedHalfEdge, leftParentEdge, eventLocation);
                    
                    if (intercept.Y != float.NegativeInfinity)//probably don't need this check?
                    {
                        //we want circle event location to be droped lower by radius to x val
                        //we want to calculate radius and store it with the event
                        //we add starting point to the halfedge that belongs to the newArc
                        //halfEdge.Start = intercept;
                        float radius = Vector2.Distance((arcs[leftCreatedHalfEdge.LeftArcIndex]).Focus, intercept);
                        intercept.Y -= radius;
                        VEvent newCircleEvent = new VEvent(intercept, EventType.CircleEvent);
                        priorityQueue.Enqueue(newCircleEvent);
                        events.Add(newCircleEvent, new VEventInfo()
                        {
                            Radius = radius,
                            VEdgeOneIndex = leftParentIndex, //pk pl
                            VEdgeTwoIndex = arcIndex, //pl pm //rChild?
                        });

                        //add to leafs
                        arc = arcs[leftCreatedHalfEdge.LeftArcIndex];
                        arc.CircleEventLocations.Add(newCircleEvent);
                        //beachTree[edgeOne.LeftArcIndex] = arc;

                        arc = arcs[leftCreatedHalfEdge.RightArcIndex];
                        arc.CircleEventLocations.Add(newCircleEvent);
                        //beachTree[edgeOne.LeftArcIndex] = arc;

                        arc = arcs[leftParentEdge.LeftArcIndex];
                        arc.CircleEventLocations.Add(newCircleEvent);
                        //beachTree[edgeTwo.RightArcIndex] = arc;
                    }
                }

                VEdge rightCreatedHalfEdge = edges[beachTree[rChild].EdgeIndex];
                int rightParentIndex = getFirstRightParentIndex(arcIndex);
                //if(rightParentIndex == -1) -> then there is no need to check for edges on the right
                if (rightParentIndex >= 0)
                {
                    VEdge rightParentEdge = edges[beachTree[rightParentIndex].EdgeIndex];//(VEdge)beachTree[parentIndex];
                    Vector2 intercept = getRayIntercept(rightCreatedHalfEdge, rightParentEdge, eventLocation);
                    if (intercept.Y != float.NegativeInfinity)
                    {
                        //we want circle event location to be droped lower by radius to x val
                        //we want to calculate radius and store it with the event
                        //we add starting point to the halfedge that belongs to the newArc
                        //twinHalfEdge.Start = intercept;
                        float radius = Vector2.Distance((arcs[rightCreatedHalfEdge.LeftArcIndex]).Focus, intercept);
                        intercept.Y -= radius;
                        VEvent newCircleEvent = new VEvent(intercept, EventType.CircleEvent);
                        priorityQueue.Enqueue(newCircleEvent);
                        events.Add(newCircleEvent, new VEventInfo()
                        {
                            Radius = radius,
                            VEdgeOneIndex = rightParentIndex, //pk pl
                            VEdgeTwoIndex = rChild, //pl pm 
                        });

                        //add to leafs
                        arc = arcs[rightCreatedHalfEdge.LeftArcIndex];
                        arc.CircleEventLocations.Add(newCircleEvent);
                        //beachTree[edgeOne.LeftArcIndex] = arc;

                        arc = arcs[rightCreatedHalfEdge.RightArcIndex];
                        arc.CircleEventLocations.Add(newCircleEvent);
                        //beachTree[edgeOne.LeftArcIndex] = arc;

                        arc = arcs[rightParentEdge.RightArcIndex];
                        arc.CircleEventLocations.Add(newCircleEvent);
                        //beachTree[edgeTwo.RightArcIndex] = arc;
                    }
                }
            }
        }

        private Vector2 getRayIntercept(VEdge edgeOne, VEdge edgeTwo, Vector2 eventLocation)
        {
            //VectorEquation vecEQ = midPointOne + vectorSlopeOne * t where t >= 0
            VArc LeftArc = arcs[edgeOne.LeftArcIndex];
            VArc RightArc = arcs[edgeOne.RightArcIndex];
            float x1 = LeftArc.Focus.X;
            float y1 = LeftArc.Focus.Y;
            float x2 = RightArc.Focus.X;
            float y2 = RightArc.Focus.Y;
            Vector2 vectorSlopeOne = new Vector2(y2 - y1, x1 - x2);
            // instead of midpoint, it should be at the relavant intersection of the two arcs.
            //Vector2 midPointOne = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);
            float startingPointOneX = getPorabolaRightIntercept(LeftArc, RightArc, eventLocation);
            float startingPointOneY = solveForY(LeftArc, startingPointOneX, eventLocation);
            if (float.IsNaN(startingPointOneY) || float.IsInfinity(startingPointOneY))
            {
                startingPointOneY = solveForY(RightArc, startingPointOneX, eventLocation);
            }
            Vector2 startPointOne = new Vector2(startingPointOneX, startingPointOneY);

            LeftArc = arcs[edgeTwo.LeftArcIndex];
            RightArc = arcs[edgeTwo.RightArcIndex];
            x1 = LeftArc.Focus.X;
            y1 = LeftArc.Focus.Y;
            x2 = RightArc.Focus.X;
            y2 = RightArc.Focus.Y;
            Vector2 vectorSlopeTwo = new Vector2(y2 - y1, x1 - x2);
            // instead of midpoint, it should be at the relavant intersection of the two arcs.
            //Vector2 midPointTwo = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);
            float startingPointTwoX = getPorabolaRightIntercept(LeftArc, RightArc, eventLocation);
            float startingPointTwoY = solveForY(LeftArc, startingPointTwoX, eventLocation);
            if (float.IsNaN(startingPointTwoY) || float.IsInfinity(startingPointTwoY))
            {
                startingPointTwoY = solveForY(RightArc, startingPointTwoX, eventLocation);
            }
            Vector2 startPointTwo = new Vector2(startingPointTwoX, startingPointTwoY);


            float[,] GMatrix = { { vectorSlopeOne.X, -vectorSlopeTwo.X, (startPointTwo.X - startPointOne.X) },
                                { vectorSlopeOne.Y, -vectorSlopeTwo.Y, (startPointTwo.Y - startPointOne.Y) }};

            GaussianElim.SolutionResult result = GaussianElim.Solve(GMatrix, GMatrix.GetLength(0));
            if (result == GaussianElim.SolutionResult.OneSolution)
            {
                float parameterResultOne = GMatrix[0, 2] / GMatrix[0, 0];
                float parameterResultTwo = GMatrix[1, 2] / GMatrix[1, 1];
                if (parameterResultOne >= 0 && parameterResultTwo >= 0)
                {
                    Vector2 foo = new Vector2(startPointOne.X + vectorSlopeOne.X * parameterResultOne, startPointOne.Y + vectorSlopeOne.Y * parameterResultOne);
                    Vector2 bar = new Vector2(startPointTwo.X + vectorSlopeTwo.X * parameterResultTwo, startPointTwo.Y + vectorSlopeTwo.Y * parameterResultTwo);
                    return bar;
                    //return new Vector3(midPointOne.x + vectorSlopeOne.x * parameterResultOne, midPointOne.y + vectorSlopeOne.y * parameterResultOne);
                }
                else
                {
                    return new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                }
            }
            else
            {
                return new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            }
        }

        internal void HandleCircleEvent(VEvent circleEvent, List<VHalfEdge> halfEdges, HalfEdgeTracker halfEdgeTracker, VertexTracker vertexTracker, ref Dictionary<VEvent, VEventInfo> events, ref PriorityQueue priorityQueue)
        {
            //Add vertex to corresponding edge record
            VEventInfo evnt = events[circleEvent];
            VEdge vEdgeOne = edges[beachTree[evnt.VEdgeOneIndex].EdgeIndex];//(VEdge)beachTree[evnt.VEdgeOneIndex];
            VEdge vEdgeTwo = edges[beachTree[evnt.VEdgeTwoIndex].EdgeIndex];//(VEdge)beachTree[evnt.VEdgeTwoIndex];

            Vector2 vertex = new Vector2(circleEvent.EventLocation.X, circleEvent.EventLocation.Y + evnt.Radius);
            Console.WriteLine($"Adding vertex at {vertex}");
            vertexTracker.Add(vertex);

            //The two halfEdges that are colliding
            VHalfEdge halfEdgeOne = vEdgeOne.HalfEdge;
            halfEdgeTracker.SetEnd(halfEdgeOne, vertex);

            VHalfEdge halfEdgeTwo = vEdgeTwo.HalfEdge;
            halfEdgeTracker.SetEnd(halfEdgeTwo, vertex);

            

            //Deleting disappearing arc
            //Create new edge record
            //replace edgeOne with the new edge
            //replace edgeTwo with the opposite branch

            //VEdge newEdge = new VEdge(vEdgeOne.LeftArcIndex, vEdgeTwo.RightArcIndex);
            //beachTree[evnt.VEdgeOneIndex].EdgeIndex = edges.Count;
            //edges.Add(newEdge);
            //copyTree(evnt.VEdgeTwoIndex, getSibling(evnt.VEdgeTwoIndex));

            VEdge newEdge;
            VHalfEdge newHalfEdge = new VHalfEdge();
            VHalfEdge newHalfEdgeTwin = new VHalfEdge();
            halfEdges.Add(newHalfEdge);
            halfEdges.Add(newHalfEdgeTwin);
            halfEdgeTracker.AddPair(newHalfEdge, newHalfEdgeTwin);
            halfEdgeTracker.SetEnd(newHalfEdgeTwin, vertex);
            if (halfEdgeOne.Tile == halfEdgeTwo.Twin.Tile)
            {
                halfEdgeOne.Next = halfEdgeTwo.Twin;
                halfEdgeTwo.Twin.Prev = halfEdgeOne;
                halfEdgeTwo.Next = newHalfEdge;
                newHalfEdge.Prev = halfEdgeTwo;
                newHalfEdge.Tile = halfEdgeTwo.Tile;
                newHalfEdgeTwin.Next = halfEdgeOne.Twin;
                halfEdgeOne.Twin.Prev = newHalfEdgeTwin;
                newHalfEdgeTwin.Tile = halfEdgeOne.Twin.Tile;
                newHalfEdge.Tile.Edges.AddLast(newHalfEdge);
                newHalfEdgeTwin.Tile.Edges.AddLast(newHalfEdgeTwin);
            }
            else
            {
                halfEdgeOne.Next = newHalfEdge;
                newHalfEdge.Prev = halfEdgeOne;
                newHalfEdgeTwin.Next = halfEdgeTwo.Twin;
                halfEdgeTwo.Twin.Prev = newHalfEdgeTwin;
                halfEdgeTwo.Next = halfEdgeOne.Twin;
                halfEdgeOne.Twin.Prev = halfEdgeTwo;
                newHalfEdge.Tile = halfEdgeOne.Tile;
                newHalfEdgeTwin.Tile = halfEdgeTwo.Twin.Tile;
                newHalfEdge.Tile.Edges.AddLast(newHalfEdge);
                newHalfEdgeTwin.Tile.Edges.AddLast(newHalfEdgeTwin);
            }
            
            // new stuff
            int arcIndexToDelete = -1;
            if (vEdgeOne.RightArcIndex == vEdgeTwo.LeftArcIndex)
            {
                arcIndexToDelete = vEdgeOne.RightArcIndex;
                newEdge = new VEdge(vEdgeOne.LeftArcIndex, vEdgeTwo.RightArcIndex, newHalfEdge);
                if (evnt.VEdgeOneIndex < evnt.VEdgeTwoIndex)
                {
                    beachTree[evnt.VEdgeOneIndex].EdgeIndex = edges.Count;
                    beachTree[evnt.VEdgeOneIndex].IsEdge = true;
                    edges.Add(newEdge);
                    int leftChildIndex = getLeftChildIndex(evnt.VEdgeTwoIndex);
                    beachTree[leftChildIndex] = null;
                    int siblingIndex = getSibling(leftChildIndex);
                    copyTree(evnt.VEdgeTwoIndex, siblingIndex, ref events);
                }
                else
                {
                    beachTree[evnt.VEdgeTwoIndex].EdgeIndex = edges.Count;
                    beachTree[evnt.VEdgeTwoIndex].IsEdge = true;
                    edges.Add(newEdge);
                    int rightChildIndex = getRightChildIndex(evnt.VEdgeOneIndex);
                    beachTree[rightChildIndex] = null;
                    int siblingIndex = getSibling(rightChildIndex);
                    copyTree(evnt.VEdgeOneIndex, siblingIndex, ref events);
                }
            }
            if (vEdgeOne.LeftArcIndex == vEdgeTwo.RightArcIndex)
            {
                arcIndexToDelete = vEdgeOne.LeftArcIndex;
                newEdge = new VEdge(vEdgeTwo.LeftArcIndex, vEdgeOne.RightArcIndex, newHalfEdge);
                if (evnt.VEdgeOneIndex < evnt.VEdgeTwoIndex)
                {
                    beachTree[evnt.VEdgeOneIndex].EdgeIndex = edges.Count;
                    beachTree[evnt.VEdgeOneIndex].IsEdge = true;
                    edges.Add(newEdge);
                    int rightChildIndex = getRightChildIndex(evnt.VEdgeTwoIndex);
                    beachTree[rightChildIndex] = null;
                    int siblingIndex = getSibling(rightChildIndex);
                    copyTree(evnt.VEdgeTwoIndex, siblingIndex, ref events);
                }
                else
                {
                    beachTree[evnt.VEdgeTwoIndex].EdgeIndex = edges.Count;
                    beachTree[evnt.VEdgeTwoIndex].IsEdge = true;
                    edges.Add(newEdge);
                    int leftChildIndex = getLeftChildIndex(evnt.VEdgeOneIndex);
                    beachTree[leftChildIndex] = null;
                    int siblingIndex = getSibling(leftChildIndex);
                    copyTree(evnt.VEdgeOneIndex, siblingIndex, ref events);
                }
            }
            

            //need to look left and right,
            events[circleEvent].Deleted = true;//look here... 10-26
            //evnt.VEdgeOneIndex
            int newEdgeIndex = Math.Min(evnt.VEdgeOneIndex, evnt.VEdgeTwoIndex);
            //int parentEdgeOfNewEdgeIndex = getParentIndex(newEdgeIndex);
            int parentEdgeOfNewEdgeIndex = getClosestLeftAncestor(newEdgeIndex);
            int closestRightAncestorIndex = getClosestRightAncestor(newEdgeIndex);
            int closestLeftEdgeIndex = getClosestLeftEdgeIndex(newEdgeIndex);
            int closestRightEdgeIndex = getClosestRightEdgeIndex(newEdgeIndex);

            //Check the new triplets for potential circle events
            if (parentEdgeOfNewEdgeIndex != newEdgeIndex)
            {
                newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];//probably not a need line?
                Vector2 intercept = getRayIntercept(newEdge, edges[beachTree[parentEdgeOfNewEdgeIndex].EdgeIndex], circleEvent.EventLocation);
                if (intercept.Y != float.NegativeInfinity && intercept != vertex)
                {
                    VEdge parentEdgeOfNewEdge = edges[beachTree[parentEdgeOfNewEdgeIndex].EdgeIndex];
                    float radius = Vector2.Distance(arcs[parentEdgeOfNewEdge.LeftArcIndex].Focus, intercept);
                    intercept.Y -= radius;
                    VEvent newCircleEvent = new VEvent(intercept, EventType.CircleEvent);
                    priorityQueue.Enqueue(newCircleEvent);

                    events.Add(newCircleEvent, new VEventInfo()
                    {
                        Radius = radius,
                        VEdgeOneIndex = parentEdgeOfNewEdgeIndex, //pk pl
                        VEdgeTwoIndex = newEdgeIndex, //pl pm 
                    });

                    //add to leafs
                    VArc arc = arcs[parentEdgeOfNewEdge.LeftArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);
                    //beachTree[parentEdgeOfNewEdge.LeftArcIndex] = arc;

                    arc = arcs[parentEdgeOfNewEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);
                    //beachTree[parentEdgeOfNewEdge.LeftArcIndex] = arc;

                    newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];

                    arc = arcs[newEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);
                    //beachTree[newEdge.RightArcIndex] = arc;
                }
            }
            if (closestLeftEdgeIndex != newEdgeIndex)//here
            {
                newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];
                Vector2 intercept = getRayIntercept(newEdge, edges[beachTree[closestLeftEdgeIndex].EdgeIndex], circleEvent.EventLocation);
                if (intercept.Y != float.NegativeInfinity && intercept != vertex)
                {
                    VEdge closestLeftEdgeOfNewEdge = edges[beachTree[closestLeftEdgeIndex].EdgeIndex];
                    float radius = Vector2.Distance(arcs[closestLeftEdgeOfNewEdge.LeftArcIndex].Focus, intercept);
                    intercept.Y -= radius;
                    VEvent newCircleEvent = new VEvent(intercept, EventType.CircleEvent);
                    priorityQueue.Enqueue(newCircleEvent);

                    events.Add(newCircleEvent, new VEventInfo()
                    {
                        Radius = radius,
                        VEdgeOneIndex = newEdgeIndex,
                        VEdgeTwoIndex = closestLeftEdgeIndex,
                    });

                    //add to leafs
                    VArc arc = arcs[closestLeftEdgeOfNewEdge.LeftArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);

                    arc = arcs[closestLeftEdgeOfNewEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);

                    newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];

                    arc = arcs[newEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);
                }
            }
            
            if (closestRightEdgeIndex != newEdgeIndex)
            {
                newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];
                Vector2 intercept = getRayIntercept(newEdge, edges[beachTree[closestRightEdgeIndex].EdgeIndex], circleEvent.EventLocation);
                if (intercept.Y != float.NegativeInfinity && intercept != vertex)
                {
                    VEdge closestRightEdgeOfNewEdge = edges[beachTree[closestRightEdgeIndex].EdgeIndex];
                    float radius = Vector2.Distance(arcs[closestRightEdgeOfNewEdge.LeftArcIndex].Focus, intercept);
                    intercept.Y -= radius;
                    VEvent newCircleEvent = new VEvent(intercept, EventType.CircleEvent);
                    priorityQueue.Enqueue(newCircleEvent);

                    events.Add(newCircleEvent, new VEventInfo()
                    {
                        Radius = radius,
                        VEdgeOneIndex = newEdgeIndex,
                        VEdgeTwoIndex = closestRightEdgeIndex,
                    });

                    //add to leafs
                    VArc arc = arcs[closestRightEdgeOfNewEdge.LeftArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);

                    arc = arcs[closestRightEdgeOfNewEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);

                    newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];

                    arc = arcs[newEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);
                }
            }
            if (closestRightAncestorIndex != newEdgeIndex)
            {
                newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];
                Vector2 intercept = getRayIntercept(newEdge, edges[beachTree[closestRightAncestorIndex].EdgeIndex], circleEvent.EventLocation);
                if (intercept.Y != float.NegativeInfinity && intercept != vertex)
                {
                    VEdge closestRightAncestorOfNewEdge = edges[beachTree[closestRightAncestorIndex].EdgeIndex];
                    float radius = Vector2.Distance(arcs[closestRightAncestorOfNewEdge.LeftArcIndex].Focus, intercept);
                    intercept.Y -= radius;
                    VEvent newCircleEvent = new VEvent(intercept, EventType.CircleEvent);
                    priorityQueue.Enqueue(newCircleEvent);

                    events.Add(newCircleEvent, new VEventInfo()
                    {
                        Radius = radius,
                        VEdgeOneIndex = newEdgeIndex,
                        VEdgeTwoIndex = closestRightAncestorIndex,
                    });

                    //add to leafs
                    VArc arc = arcs[closestRightAncestorOfNewEdge.LeftArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);

                    arc = arcs[closestRightAncestorOfNewEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);

                    newEdge = edges[beachTree[newEdgeIndex].EdgeIndex];

                    arc = arcs[newEdge.RightArcIndex];
                    arc.CircleEventLocations.Add(newCircleEvent);
                }
            }
        }
        private int getClosestRightAncestor(int edgeIndex)
        {
            int currentDescendant = edgeIndex;

            while (hasParent(currentDescendant))
            {
                int parentIndex = getParentIndex(currentDescendant);

                if (getLeftChildIndex(parentIndex) == currentDescendant)
                {
                    return parentIndex;
                }

                currentDescendant = parentIndex;
            }

            return edgeIndex;
        }

        private int getClosestLeftAncestor(int edgeIndex)
        {
            int currentDescendant = edgeIndex;

            while (hasParent(currentDescendant))
            {
                int parentIndex = getParentIndex(currentDescendant);

                if (getRightChildIndex(parentIndex) == currentDescendant)
                {
                    return parentIndex;
                }

                currentDescendant = parentIndex;
            }

            return edgeIndex;
        }

        private int getClosestLeftEdgeIndex(int edgeIndex)
        {
            int retVal = edgeIndex;
            int leftChildIndex = getLeftChildIndex(edgeIndex);
            if (beachTree[leftChildIndex] != null && beachTree[leftChildIndex].IsEdge)
            {
                retVal = leftChildIndex;
                while (hasRightChild(retVal))
                {
                    retVal = getRightChildIndex(retVal);
                    if (beachTree[retVal].IsEdge == false)
                    {
                        return getParentIndex(retVal);
                    }
                }
            }
            return retVal;
        }

        private int getClosestRightEdgeIndex(int edgeIndex)
        {
            int retVal = edgeIndex;
            int leftChildIndex = getRightChildIndex(edgeIndex);
            if (beachTree[leftChildIndex] != null && beachTree[leftChildIndex].IsEdge)
            {
                retVal = leftChildIndex;
                while (hasLeftChild(retVal))
                {
                    retVal = getLeftChildIndex(retVal);
                    if (beachTree[retVal].IsEdge == false)
                    {
                        return getParentIndex(retVal);
                    }
                }
            }
            return retVal;
        }

        private IEnumerable InorderEdges(BeachLineItem[] beachTree, int index)
        {
            if (beachTree[index] != null || beachTree[index].IsEdge)
            {
                foreach (int edgeIndex in InorderEdges(beachTree, getLeftChildIndex(index)))
                {
                    yield return edgeIndex;
                }
                yield return edges[beachTree[index].EdgeIndex];
                foreach (int edgeIndex in InorderEdges(beachTree, getRightChildIndex(index)))
                {
                    yield return edgeIndex;
                }
            }
        }
        private int getSibling(int siblingIndex)
        {
            int parentIndex = getParentIndex(siblingIndex);
            if (getLeftChildIndex(parentIndex) == siblingIndex)
            {
                return getRightChildIndex(parentIndex);
            }
            else
            {
                return getLeftChildIndex(parentIndex);
            }
        }

        private void copyTree(int target, int treeStart, ref Dictionary<VEvent, VEventInfo> events)
        {
            if (beachTree[treeStart] == null)
            {
                return;
            }

            beachTree[target] = beachTree[treeStart];
            if (beachTree[treeStart].IsEdge)
            {
                //update related circle events to edge
                VEdge edge = edges[beachTree[treeStart].EdgeIndex];
                VArc arc = arcs[edge.LeftArcIndex];
                foreach (VEvent circleEventLocation in arc.CircleEventLocations)
                {
                    if (events[circleEventLocation].VEdgeTwoIndex == treeStart)
                    {
                        events[circleEventLocation].VEdgeTwoIndex = target;
                    }
                    else if (events[circleEventLocation].VEdgeOneIndex == treeStart)
                    {
                        events[circleEventLocation].VEdgeOneIndex = target;
                    }
                }
            }
            beachTree[treeStart] = null;

            copyTree(getLeftChildIndex(target), getLeftChildIndex(treeStart), ref events);

            copyTree(getRightChildIndex(target), getRightChildIndex(treeStart), ref events);
        }

        //locate the existing arc (if any) that is above the new site
        //eventIntersection is the location of intersection between the event in question with the most relavant arc on the beachline
        private int getExistingArc(BeachLineItem[] beachTree, Vector2 eventLocation)
        {
            if (beachTree[0] == null)
            {
                return -1;
            }
            if (!beachTree[0].IsEdge)
            {
                return 0;
            }
            int nodeIndex = 0;
            bool arcNotFound = true;
            while (arcNotFound)
            {
                ensureExtraCapacity(nodeIndex);
                if (!isLeafNode(nodeIndex))
                {
                    VEdge edgeNode = edges[beachTree[nodeIndex].EdgeIndex];
                    float rightXIntercept = getPorabolaRightIntercept(arcs[edgeNode.LeftArcIndex], arcs[edgeNode.RightArcIndex], eventLocation);
                    
                    ensureExtraCapacity(getLeftChildIndex(nodeIndex));
                    if (rightXIntercept < eventLocation.X && !float.IsNaN(rightXIntercept))
                    {
                        nodeIndex = getRightChildIndex(nodeIndex);
                    }
                    else if (!isLeafNode(getLeftChildIndex(nodeIndex)))
                    {
                        int leftChildIndex = getLeftChildIndex(nodeIndex);
                        VEdge leftEdgeNode = edges[beachTree[leftChildIndex].EdgeIndex];
                        float leftIntercept = getPorabolaRightIntercept(arcs[leftEdgeNode.LeftArcIndex], arcs[leftEdgeNode.RightArcIndex], eventLocation);
                        if (leftIntercept < eventLocation.X)
                        {
                            //needs to return an index on the beachtree
                            //not an arc index
                            //looking for the beachline index that points to the same arc index as 'targetArcIndex'
                            nodeIndex = getRightChildIndex(leftChildIndex);

                            //return leftEdgeNode.RightArcIndex;
                        }
                        else
                        {
                            nodeIndex = leftChildIndex;
                        }
                    }
                    else
                    {
                        //arcNotFound = false;
                        //float rightYIntercept = solveForY(arcs[edgeNode.LeftArcIndex], rightXIntercept, eventLocation);
                        //eventIntersection.Y = solveForY(arcs[beachTree[nodeIndex].ArcIndex], eventIntersection.X, eventLocation);
                        return getLeftChildIndex(nodeIndex);
                    }
                }
                else
                {
                    //arcNotFound = false;
                    return nodeIndex;
                }
            }
            return -1;
        }

        //going from left to right on the curve of the left Arc, return the first intersection with the right Arc
        private float getPorabolaRightIntercept(VArc leftArc, VArc rightArc, Vector2 eventLocation)
        {
            if (leftArc.Focus.Y == eventLocation.Y)
            {
                return leftArc.Focus.X;
            }
            else if (rightArc.Focus.Y == eventLocation.Y)
            {
                return rightArc.Focus.X;
            }

            //y = ((x - a)^2 + b^2 - c^2)/(2(b - c))

            double ANot = leftArc.Focus.X;
            double BNot = leftArc.Focus.Y;

            double APrime = rightArc.Focus.X;
            double BPrime = rightArc.Focus.Y;

            double C = eventLocation.Y;

            double E = Math.Pow(ANot, 2) + Math.Pow(BNot, 2) - Math.Pow(C, 2);
            double F = 2 * ANot;
            double G = 2 * (BNot - C);
            double H = 1 / G;
            double I = (F / G) * -1;
            double J = E / G;

            double K = Math.Pow(APrime, 2) + Math.Pow(BPrime, 2) - Math.Pow(C, 2);
            double L = 2 * APrime;
            double M = 2 * (BPrime - C);
            double N = 1 / M;
            double O = (L / M) * -1;
            double P = K / M;
            //HX^2 + IX + J = NX^2 + OX + P
            //(H - N)X^2 + (I - O)X + (J - P)
            //QX^2 + RX + S = 0
            double Q = N - H;
            double R = O - I;
            double S = P - J;
            if (Q == 0)
            {
                //RX + S = 0
                //X = -S / R;
                if ((-S / R) < leftArc.Focus.X)
                {
                    return float.NaN;
                }
                else
                {
                    return (float)(-S / R);
                }
            }
            double x1;
            double x2;
            QuadraticEquation(out x1, out x2, Q, R, S);
            if (Double.IsNaN(x1) || Double.IsInfinity(x1))
            {
                //There is less than two solutions
                if (Double.IsNaN(x2) || Double.IsInfinity(x2))
                {
                    //There is no solution
                    return float.NaN;
                }
                else
                {
                    //There is one solution
                    //return (float)x2;
                    if (leftArc.Focus.Y > rightArc.Focus.Y)
                    {
                        return float.NaN;
                    }
                    else if (leftArc.Focus.Y == rightArc.Focus.Y)
                    {
                        if (leftArc.Focus.X < rightArc.Focus.X)
                        {
                            return float.NaN;
                        }
                        else
                        {
                            return (float)x2;
                        }
                    }
                    else
                    {
                        return (float)x2;
                    }
                }
            }
            else
            {
                //There might be two solutions
                if (Double.IsNaN(x2) || Double.IsInfinity(x2))
                {
                    //There is one solution
                    //return (float)x1;
                    if (leftArc.Focus.Y > rightArc.Focus.Y)
                    {
                        return (float)x1;
                    }
                    else if (leftArc.Focus.Y == rightArc.Focus.Y)
                    {
                        if (leftArc.Focus.X < rightArc.Focus.X)
                        {
                            return (float)x1;
                        }
                        else
                        {
                            return float.NaN;
                        }
                    }
                    else
                    {
                        return (float)x1;
                    }
                }
                else
                {
                    //There are two solutions
                    if (leftArc.Focus.Y > rightArc.Focus.Y)
                    {
                        return (float)Math.Min(x1, x2);
                    }
                    else
                    {
                        return (float)Math.Max(x1, x2);
                    }
                }
            }
        }

        private float solveForY(VArc arc, float x, Vector2 eventLocation)
        {
            double xFocus = arc.Focus.X;
            double yFocus = arc.Focus.Y;

            double directrix = eventLocation.Y;

            double K = Math.Pow(xFocus, 2) + Math.Pow(yFocus, 2) - Math.Pow(directrix, 2);
            double L = 2 * xFocus;
            double M = 2 * (yFocus - directrix);
            double N = 1 / M;
            double O = (L / M) * -1;
            double P = K / M;
            //NX ^ 2 + OX + P

            return (float)(N * Math.Pow(x, 2) + O * x + P);
        }

        private void QuadraticEquation(out double x1, out double x2, double A, double B, double C)
        {
            x1 = (-B + Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);
            x2 = (-B - Math.Sqrt(Math.Pow(B, 2) - 4 * A * C)) / (2 * A);
        }

        private void removeRelatedCircleEvent(int arcIndex, Vector2 VertexEventSite, ref Dictionary<VEvent, VEventInfo> events)
        {
            foreach (VEvent circleEventSite in (arcs[beachTree[arcIndex].ArcIndex]).CircleEventLocations)
            {
                bool test = events.ContainsKey(circleEventSite);//<--- delete
                Vector2 cricleEventSiteCenter = new Vector2()
                {
                    X = circleEventSite.EventLocation.X,
                    Y = circleEventSite.EventLocation.Y + events[circleEventSite].Radius
                };
                float radius = events[circleEventSite].Radius;
                if (events[circleEventSite].Deleted != true)
                {
                    float distanceToCircleCenter = Vector2.Distance(cricleEventSiteCenter, VertexEventSite);
                    if (distanceToCircleCenter < radius - Epsilon)
                    {
                        events[circleEventSite].Deleted = true;
                        return;
                    }
                }
            }
        }

        private void ensureExtraCapacity(int index)
        {
            //2 * parentIndex + 2
            if (2 * index + 2 >= capacity)
            {
                BeachLineItem[] newArray = new BeachLineItem[2 * index + 3];
                Array.Copy(beachTree, newArray, beachTree.Length);
                beachTree = newArray;
                capacity = beachTree.Length;
            }
        }

        private BeachTreeDisplay BuildBeachTreeDisplay(int index)
        {
            if (!HasBeachTreeNode(index))
            {
                return null;
            }

            string label = FormatBeachTreeNode(index);
            BeachTreeDisplay left = BuildBeachTreeDisplay(getLeftChildIndex(index));
            BeachTreeDisplay right = BuildBeachTreeDisplay(getRightChildIndex(index));

            if (left == null && right == null)
            {
                return new BeachTreeDisplay(new List<string> { label }, label.Length, label.Length / 2);
            }

            if (left == null)
            {
                return BuildRightOnlyBeachTreeDisplay(label, right);
            }

            if (right == null)
            {
                return BuildLeftOnlyBeachTreeDisplay(label, left);
            }

            return BuildTwoChildBeachTreeDisplay(label, left, right);
        }

        private BeachTreeDisplay BuildTwoChildBeachTreeDisplay(string label, BeachTreeDisplay left, BeachTreeDisplay right)
        {
            int middleGap = label.Length + 2;
            int width = left.Width + middleGap + right.Width;
            int rootOffset = left.Width + 1 + label.Length / 2;
            List<string> lines = new List<string>();

            lines.Add(new string(' ', left.Width + 1) + label + new string(' ', right.Width + 1));
            lines.Add(
                new string(' ', left.RootOffset + 1) +
                "/" +
                new string(' ', left.Width - left.RootOffset - 1 + label.Length + right.RootOffset) +
                "\\" +
                new string(' ', right.Width - right.RootOffset));

            int height = Math.Max(left.Lines.Count, right.Lines.Count);
            for (int i = 0; i < height; i++)
            {
                string leftLine = i < left.Lines.Count ? PadRightTo(left.Lines[i], left.Width) : new string(' ', left.Width);
                string rightLine = i < right.Lines.Count ? PadRightTo(right.Lines[i], right.Width) : new string(' ', right.Width);

                lines.Add(leftLine + new string(' ', middleGap) + rightLine);
            }

            return new BeachTreeDisplay(lines, width, rootOffset);
        }

        private BeachTreeDisplay BuildLeftOnlyBeachTreeDisplay(string label, BeachTreeDisplay left)
        {
            int middleGap = label.Length + 1;
            int width = left.Width + middleGap;
            int rootOffset = left.Width + 1 + label.Length / 2;
            List<string> lines = new List<string>();

            lines.Add(new string(' ', left.Width + 1) + label);
            lines.Add(new string(' ', left.RootOffset + 1) + "/" + new string(' ', width - left.RootOffset - 2));

            foreach (string leftLine in left.Lines)
            {
                lines.Add(PadRightTo(leftLine, left.Width) + new string(' ', middleGap));
            }

            return new BeachTreeDisplay(lines, width, rootOffset);
        }

        private BeachTreeDisplay BuildRightOnlyBeachTreeDisplay(string label, BeachTreeDisplay right)
        {
            int middleGap = label.Length + 1;
            int width = middleGap + right.Width;
            int rootOffset = label.Length / 2;
            int branchOffset = label.Length + right.RootOffset;
            List<string> lines = new List<string>();

            lines.Add(label + new string(' ', right.Width + 1));
            lines.Add(new string(' ', branchOffset) + "\\" + new string(' ', width - branchOffset - 1));

            foreach (string rightLine in right.Lines)
            {
                lines.Add(new string(' ', middleGap) + PadRightTo(rightLine, right.Width));
            }

            return new BeachTreeDisplay(lines, width, rootOffset);
        }

        private string FormatBeachTreeNode(int index)
        {
            BeachLineItem item = beachTree[index];
            string itemType = item.IsEdge ? "E" : "A";
            int targetIndex = item.IsEdge ? item.EdgeIndex : item.ArcIndex;

            return $"[{index}] {itemType}[{targetIndex}]";
        }

        private static string PadRightTo(string value, int width)
        {
            if (value.Length >= width)
            {
                return value;
            }

            return value + new string(' ', width - value.Length);
        }

        private bool HasBeachTreeNode(int index)
        {
            return index >= 0 && index < beachTree.Length && beachTree[index] != null;
        }

        private bool hasParent(int index)
        {
            return index > 0;
        }
        private int getParentIndex(int childIndex)
        {
            return (childIndex - 1) / 2;
        }
        private bool isLeafNode(int index)
        {
            return !hasLeftChild(index) && !hasRightChild(index);
        }

        private bool hasLeftChild(int index)
        {
            return beachTree[getLeftChildIndex(index)] != null;
        }

        private bool hasRightChild(int index)
        {
            return beachTree[getRightChildIndex(index)] != null;
        }

        private int getLeftChildIndex(int parentIndex)
        {
            return 2 * parentIndex + 1;
        }

        private int getRightChildIndex(int parentIndex)
        {
            return 2 * parentIndex + 2;
        }

        //get parent who's passed childindex is somewhere on the left from the prespective of said parent
        private int getFirstRightParentIndex(int index)
        {
            /*int parentIndex = getParentIndex(childIndex);
            if(parentIndex == 0)
            {
                return -1;
            }
            if (getRightChildIndex(parentIndex) != childIndex)
            {
                return getFirstRightParentIndex(parentIndex);
            }
            else
            {
                return parentIndex;
            }*/
            int retVal = -1;
            bool rightParentFound = false;
            while (!rightParentFound && hasParent(index))
            {
                int parentIndex = getParentIndex(index);
                if (getLeftChildIndex(parentIndex) == index)
                {
                    retVal = parentIndex;
                    rightParentFound = true;
                }
                else
                {
                    index = parentIndex;
                }

            }
            return retVal;
        }

        //get parent who's passed childindex is somewhere on the right from the prespective of said parent
        private int getFirstLeftParentIndex(int index)
        {
            //TODO
            int retVal = -1;
            bool leftParentFound = false;
            while (!leftParentFound && hasParent(index))
            {
                int parentIndex = getParentIndex(index);
                if (getRightChildIndex(parentIndex) == index)
                {
                    retVal = parentIndex;
                    leftParentFound = true;
                }
                else
                {
                    index = parentIndex;
                }

            }
            return retVal;
        }

        private sealed class BeachTreeDisplay
        {
            public List<string> Lines { get; }
            public int Width { get; }
            public int RootOffset { get; }

            public BeachTreeDisplay(List<string> lines, int width, int rootOffset)
            {
                Lines = lines;
                Width = width;
                RootOffset = rootOffset;
            }
        }
    }
}
