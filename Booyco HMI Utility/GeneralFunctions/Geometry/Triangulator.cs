using Booyco_HMI_Utility.Geofences.Shapes;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{

    public partial struct LatLonCoord
    {
        public static class Triangulator
        {

            static readonly IndexableCyclicalLinkedList<LatLonVertex> polygonVertices = new IndexableCyclicalLinkedList<LatLonVertex>();
            static readonly IndexableCyclicalLinkedList<LatLonVertex> earVertices = new IndexableCyclicalLinkedList<LatLonVertex>();
            static readonly CyclicalList<LatLonVertex> convexVertices = new CyclicalList<LatLonVertex>();
            static readonly CyclicalList<LatLonVertex> reflexVertices = new CyclicalList<LatLonVertex>();

            public static LatLonLineSegment SideToLine(List<LatLonTriangle> Triangles, int triangleIdx, int SidePoint1, int Sidepoint2)
            {
                LatLonLineSegment sideLine = new LatLonLineSegment(Triangles[triangleIdx][SidePoint1], Triangles[triangleIdx][Sidepoint2]);
                sideLine = sideLine.orientateLeftUp();
                return sideLine;
            }

            public static void MakeLines(List<LatLonLineSegment> ll, int pCount)
            {
                ll.Clear();
                for(var i=0; i<pCount; i++)
                {
                    ll.Add(new LatLonLineSegment());
                }
            }

            public static bool AddTriangleToPolyLines(ref List<LatLonLineSegment> polyLines, List<LatLonLineSegment> triangleLines)
            {
                int atPL, atTL, atn, ati, atf;
                bool bHasCommonSide = false;
                bool result = false;
                atTL = triangleLines.Count;
                atPL = polyLines.Count;
                int[] aCommonSides = new int[3];
                if (atTL == 3)
                {
                    for (ati = 0; ati < atTL; ati++)
                    {
                        atf = -1;
                        if (atPL > 0) // If there are any sides in this polygon yet,
                        {
                            atn = 0; // Find if there exists a line in the polygon that is the exact same as the current Triangle side (ati),
                            while (atn < atPL)
                            {
                                if (polyLines[atn].Matches(triangleLines[ati])) { atf = atn; atn = atPL; } else { atn++; }
                            }
                        }
                        if (atf < 0) {       // If no such side exists yet, this side must be added to either this, or the next polygon.
                            aCommonSides[ati] = 0;
                        }
                        else
                        { // else the line inside the polygon must be removed (duplicate triangle sides are inner lines and
                            atPL--; // when all inner lines are removed, what remains is the outer polygon).
                            aCommonSides[ati] = 1;     // Mark this Side as being common (to avoid adding it later)
                            bHasCommonSide = true;
                            polyLines.RemoveAt(atf); // remove in between
                        }
                    }
                    if ((bHasCommonSide) || (atPL < 3))
                    {
                        result = true;  // If this is a new poly, or the triangle shares a line, mark to add triangle here.
                    }
                    if (result)
                    {                  // If Triangle should be aded here (minus any common sides)...
                        for (ati = 0; ati < atTL; ati++)
                        { // Walk the lines/sides list...
                            if (aCommonSides[ati] == 0)
                            { // If this is not a common side, add it...
                                LatLonLineSegment line = triangleLines[ati].Clone();
                                polyLines.Add(line);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("triangleLines not a triangle");
                }
                return result;
            }

            public static List<LatLonLineSegment> TriangleToLines(List<LatLonTriangle> triangles, int triangleIdx)
             {
                List<LatLonLineSegment> tlns = new List<LatLonLineSegment>();
                MakeLines(tlns, 3);
                tlns[0] = SideToLine(triangles, triangleIdx, 0, 1);
                tlns[1] = SideToLine(triangles, triangleIdx, 1, 2);
                tlns[2] = SideToLine(triangles, triangleIdx, 2, 0);
                return tlns;
            }

            public static List<LatLonCoord> CreatePolyFromLines(List<LatLonLineSegment> polygonLines)
            {
                List<LatLonCoord> result = new List<LatLonCoord>();
                int cpL, cpp, cpn, cpi, cpf;
                double cpLat = 0, cpLon = 0;
                cpL = polygonLines.Count;
                if (cpL > 0) {
                    cpn = 0;
                    cpf = -1;
                    while (cpn < cpL) // Walk the lines and find the Leftest (or Toppest of the Lefts) line to start to build from...
                    {
                        if (cpf < 0) { 
                            cpLon = polygonLines[cpn].A.Position.Longitude; cpLat = polygonLines[cpn].A.Position.Latitude; cpf = cpn; 
                        } // Found a line linking normally...  
                        else if (polygonLines[cpn].A.Position.Longitude < cpLon){ 
                            cpLon = polygonLines[cpn].A.Position.Longitude; cpLat = polygonLines[cpn].A.Position.Latitude; cpf = cpn; 
                        } // Found a line linking to the wrong end
                        else if ((MathHelper.DoubleEqual(polygonLines[cpn].A.Position.Longitude, cpLon)) && (polygonLines[cpn].A.Position.Latitude < cpLat)){ 
                            cpLat = polygonLines[cpn].A.Position.Latitude; cpf = cpn; 
                        }                                                                        
                        cpn++;
                    }
                    // MakePoints(result, cpL + 1);          // There are exactly n+1 points to any n connected lines.
                    cpp = 0;
                    result.Add(new LatLonCoord(cpLat, cpLon)); // Start the first polygon point on the Lat1-lon1 of Line 1.
                    while ((cpf >= 0) && (cpp < cpL))
                    { // Every time a new line (or the first line) is found (cpf>=0),    
                        cpi = cpf;                               // Set cpi at the found line index,
                        cpp++;                               // increase the polypoint index cpp,
                        cpLat = polygonLines[cpi].B.Position.Latitude;  // Pin point coordinates to the Lat2-Lon2 of the line,
                        cpLon = polygonLines[cpi].B.Position.Longitude;
                        result.Add(new LatLonCoord(cpLat, cpLon));  // set the new poly point cpp to the pinned coordinates,   
                        cpn = 0; cpf = -1;                   // Find any next line which starts exactly at the current pinned Lat2-Lon2:
                        while (cpn < cpL)
                        {
                            if (cpn != cpi)
                            {
                                if (
                                    MathHelper.DoubleEqual(polygonLines[cpn].A.Position.Latitude, cpLat) && MathHelper.DoubleEqual(polygonLines[cpn].A.Position.Longitude, cpLon)
                                )
                                {
                                    cpf = cpn; cpn = cpL;
                                }
                                else if (MathHelper.DoubleEqual(polygonLines[cpn].B.Position.Latitude, cpLat) && MathHelper.DoubleEqual(polygonLines[cpn].B.Position.Longitude, cpLon))
                                {    // Found a line linking to the wrong end,
                                    cpf = cpn; cpn = cpL;  // Mark the line as found,
                                    polygonLines[cpf] = polygonLines[cpf].swapCoordinates(); // Swap line-ending points.
                                }
                            }
                            cpn++;
                        }
                    }
                }
                result.RemoveAt(result.Count - 1);
                return result;
            }

            public static List<LatLonPolygon> TrianglesToPolygons(GeofenceTriangle[] geoFenceTriangleArray)
            {
                // convert to list
                List<LatLonTriangle> tls = new List<LatLonTriangle>();
                List<GeoFenceAreaType> tlsTypes = new List<GeoFenceAreaType>();
                List<int> tlsBearing = new List<int>();
                for (int ti=0; ti<geoFenceTriangleArray.Length; ti++)
                {
                    if (((GeoFenceAreaType)geoFenceTriangleArray[ti].Type) != GeoFenceAreaType.None)
                    {
                        tls.Add(new LatLonTriangle(
                           new LatLonVertex(new LatLonCoord(LatLonPartFromUInt32(geoFenceTriangleArray[ti].LatitudePoint1), LatLonPartFromUInt32(geoFenceTriangleArray[ti].LongitudePoint1)), 0),
                           new LatLonVertex(new LatLonCoord(LatLonPartFromUInt32(geoFenceTriangleArray[ti].LatitudePoint2), LatLonPartFromUInt32(geoFenceTriangleArray[ti].LongitudePoint2)), 1),
                           new LatLonVertex(new LatLonCoord(LatLonPartFromUInt32(geoFenceTriangleArray[ti].LatitudePoint3), LatLonPartFromUInt32(geoFenceTriangleArray[ti].LongitudePoint3)), 2)
                        ));
                        tlsTypes.Add((GeoFenceAreaType)geoFenceTriangleArray[ti].Type);
                        tlsBearing.Add((int)geoFenceTriangleArray[ti].Heading);
                    }
                }
                
                int L = tls.Count;
                List<LatLonLineSegment> polyLines, triangleLines;
                List<LatLonPolygon> polygons = new List<LatLonPolygon>();
                int newPolyHeading = 0;
                GeoFenceAreaType newPolyAreaType = GeoFenceAreaType.None;
                polyLines = new List<LatLonLineSegment>();
                if (L > 0)
                {
                    // init on first
                    for (int n = 0; n < L; n++)// Walk the Triangle List...
                    {
                        triangleLines = TriangleToLines(tls, n);
                        newPolyHeading = tlsBearing[n];
                        newPolyAreaType = tlsTypes[n];
                        if (!AddTriangleToPolyLines(ref polyLines, triangleLines)) {  // If the adder return false, we should start a new Polygon...
                            if (polyLines.Count > 0) {  // If there are any valid lines added,
                                polygons.Add(new LatLonPolygon(polyLines, newPolyHeading, newPolyAreaType));       // Create a new Polygon from the lines and push it onto the list.
                                polyLines.Clear();
                                AddTriangleToPolyLines(ref polyLines, triangleLines);   // Add the current triangle now to the new Polygon. (Nvm, the return value, this must be new)
                            }
                        }
                    }
                    if (polyLines.Count > 0)
                    {  // If there are any valid lines still, the final Polygon must be closed off...
                        polygons.Add(new LatLonPolygon(polyLines, newPolyHeading, newPolyAreaType)); // Create a new Polygon from the lines and push it onto the list.
                    }
                }
                return polygons;
            }                                  

            /// <summary>
            /// Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
            /// </summary>
            /// <param name="inputVertices">The polygon vertices in counter-clockwise winding order.</param>
            /// <param name="desiredWindingOrder">The desired output winding order.</param>
            public static List<LatLonTriangle> Triangulate(
                LatLonCoord[] inputVertices,
                WindingOrder desiredWindingOrder)
            {
                //Log("\nBeginning triangulation...");
                LatLonCoord[] outputVertices;
                int[] indices;

                List<LatLonTriangle> triangles = new List<LatLonTriangle>();

                //make sure we have our vertices wound properly
                if (DetermineWindingOrder(inputVertices) == WindingOrder.Clockwise)
                    outputVertices = ReverseWindingOrder(inputVertices);
                else
                    outputVertices = (LatLonCoord[])inputVertices.Clone();

                //clear all of the lists
                polygonVertices.Clear();
                earVertices.Clear();
                convexVertices.Clear();
                reflexVertices.Clear();

                //generate the cyclical list of vertices in the polygon
                for (int i = 0; i < outputVertices.Length; i++)
                    polygonVertices.AddLast(new LatLonVertex(outputVertices[i], i));

                //categorize all of the vertices as convex, reflex, and ear
                FindConvexAndReflexVertices();
                FindEarVertices();

                //clip all the ear vertices
                while (polygonVertices.Count > 3 && earVertices.Count > 0)
                    ClipNextEar(triangles);

                //if there are still three points, use that for the last triangle
                if (polygonVertices.Count == 3)
                    triangles.Add(new LatLonTriangle(
                        polygonVertices[0].Value,
                        polygonVertices[1].Value,
                        polygonVertices[2].Value));

                //add all of the triangle indices to the output array
                indices = new int[triangles.Count * 3];

                //move the if statement out of the loop to prevent all the
                //redundant comparisons
                if (desiredWindingOrder == WindingOrder.CounterClockwise)
                {
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        indices[(i * 3)] = triangles[i].A.Index;
                        indices[(i * 3) + 1] = triangles[i].B.Index;
                        indices[(i * 3) + 2] = triangles[i].C.Index;
                    }
                }
                else
                {
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        indices[(i * 3)] = triangles[i].C.Index;
                        indices[(i * 3) + 1] = triangles[i].B.Index;
                        indices[(i * 3) + 2] = triangles[i].A.Index;
                    }
                }

                return triangles;
            }

            /// <summary>
            /// Reverses the winding order for a set of vertices.
            /// </summary>
            /// <param name="vertices">The vertices of the polygon.</param>
            /// <returns>The new vertices for the polygon with the opposite winding order.</returns>
            public static LatLonCoord[] ReverseWindingOrder(LatLonCoord[] vertices)
            {
                // Log("\nReversing winding order...");
                LatLonCoord[] newVerts = new LatLonCoord[vertices.Length];
                newVerts[0] = vertices[0];
                for (int i = 1; i < newVerts.Length; i++)
                    newVerts[i] = vertices[vertices.Length - i];
                return newVerts;
            }


            /// <summary>
            /// Determines the winding order of a polygon given a set of vertices.
            /// </summary>
            /// <param name="vertices">The vertices of the polygon.</param>
            /// <returns>The calculated winding order of the polygon.</returns>
            public static WindingOrder DetermineWindingOrder(LatLonCoord[] vertices)
            {
                double sum = 0.0;
                for (int i = 0; i < vertices.Length; i++)
                {
                    LatLonCoord v1 = vertices[i];
                    LatLonCoord v2 = vertices[(i + 1) % vertices.Length];
                    sum += (v2.Longitude - v1.Longitude) * (v2.Latitude + v1.Latitude);
                }
                return (sum > 0.0)?WindingOrder.Clockwise:WindingOrder.CounterClockwise;
            }

            private static void ClipNextEar(ICollection<LatLonTriangle> triangles)
            {
                //find the triangle
                LatLonVertex ear = earVertices[0].Value;
                LatLonVertex prev = polygonVertices[polygonVertices.IndexOf(ear) - 1].Value;
                LatLonVertex next = polygonVertices[polygonVertices.IndexOf(ear) + 1].Value;
                triangles.Add(new LatLonTriangle(ear, next, prev));
                //remove the ear from the shape
                earVertices.RemoveAt(0);
                polygonVertices.RemoveAt(polygonVertices.IndexOf(ear));
                //validate the neighboring vertices
                ValidateAdjacentVertex(prev);
                ValidateAdjacentVertex(next);
            }

            private static void ValidateAdjacentVertex(LatLonVertex vertex)
            {
                if (reflexVertices.Contains(vertex))
                {
                    if (IsConvex(vertex))
                    {
                        reflexVertices.Remove(vertex);
                        convexVertices.Add(vertex);
                    }
                }
                if (convexVertices.Contains(vertex))
                {
                    bool wasEar = earVertices.Contains(vertex);
                    bool isEar = IsEar(vertex);

                    if (wasEar && !isEar)
                    {
                        earVertices.Remove(vertex);
                    }
                    else if (!wasEar && isEar)
                    {
                        earVertices.AddFirst(vertex);
                    }
                }
            }

            private static void FindConvexAndReflexVertices()
            {
                for (int i = 0; i < polygonVertices.Count; i++)
                {
                    LatLonVertex v = polygonVertices[i].Value;
                    if (IsConvex(v))
                    {
                        convexVertices.Add(v);
                    }
                    else
                    {
                        reflexVertices.Add(v);
                    }
                }
            }

            private static void FindEarVertices()
            {
                for (int i = 0; i < convexVertices.Count; i++)
                {
                    LatLonVertex c = convexVertices[i];
                    if (IsEar(c))
                    {
                        earVertices.AddLast(c);
                    }
                }
            }

            private static bool IsEar(LatLonVertex c)
            {
                LatLonVertex p = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
                LatLonVertex n = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;
                foreach (LatLonVertex t in reflexVertices)
                {
                    if (t.Equals(p) || t.Equals(c) || t.Equals(n))
                        continue;
                    if (LatLonTriangle.ContainsPoint(p, c, n, t))
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool IsConvex(LatLonVertex c)
            {
                LatLonVertex p = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
                LatLonVertex n = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;
                LatLonCoord d1 = LatLonCoord.Normalize(c.Position.Substract(p.Position));
                LatLonCoord d2 = LatLonCoord.Normalize(n.Position.Substract(c.Position));
                LatLonCoord n2 = new LatLonCoord(d2.Longitude, -d2.Latitude);
                return (LatLonCoord.Dot(d1, n2) <= 0f);
            }
        }
    }
}
