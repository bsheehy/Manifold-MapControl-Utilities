using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManifoldMapEdit
{
    public enum DrawEventType
    {
        InsertPoint,
        InsertGPSPoint,
        Insertline,
        InsertArea,
        UpdatePoint,
        UpdateGeom,
        UpdateGPSPoint,
        Updateline,
        UpdateArea,
        DeletePoint,
        DeleteGPSPoint,
        Deleteline,
        DeleteArea,
        MoveGeom
    }

    public enum CLEAR_EDIT_LAYER
    {
        ALL = 0,
        SNAP_POINTS = 1,
        NON_SNAP_POINTS = 2
    }

    public static class Utility
    {
        public static bool isModeTypeEditEnabled(MODETYPE_OPERATION p_MODETYPE_OPERATION)
        {
            bool bReturn = false;
            try
            {
                if (p_MODETYPE_OPERATION == MODETYPE_OPERATION.EditModeEnabled
                  || p_MODETYPE_OPERATION == MODETYPE_OPERATION.EditAddCoordinate
                  || p_MODETYPE_OPERATION == MODETYPE_OPERATION.EditMoveGeom
                  || p_MODETYPE_OPERATION == MODETYPE_OPERATION.EditDeleteCoordinate)
                {
                    bReturn = true;
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            return bReturn;
        }

        public static Manifold.Interop.Map loadManifoldMap(AxManifold.Interop.AxComponentControl p_axManifoldMap, string p_sMapPath, string p_sMapName)
        {
            Manifold.Interop.Map objReturn = null;
            bool bMapExists = false;

            try
            {
                p_axManifoldMap.set_Document(p_sMapPath);
                Manifold.Interop.ComponentSet m_ComponentSet = p_axManifoldMap.get_Document().ComponentSet;

                //Disaply _Map_Inspection if exists
                foreach (Manifold.Interop.Component objComponent in m_ComponentSet)
                {
                    switch (objComponent.Type)
                    {
                        case Manifold.Interop.ComponentType.ComponentMap:
                            if (p_sMapName.Equals(objComponent.Name))
                            {
                                //Get Map Name
                                objReturn = (Manifold.Interop.Map)objComponent;
                                p_axManifoldMap.ComponentName = objComponent.Name;
                                bMapExists = true;
                                break;
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Create new Map if required
                if (bMapExists == false)
                {
                    Manifold.Interop.ComponentSet objNewComponentSet = p_axManifoldMap.get_Document().NewComponentSet();
                    foreach (Manifold.Interop.Component objComponent in m_ComponentSet)
                    {
                        switch (objComponent.Type)
                        {
                            case Manifold.Interop.ComponentType.ComponentDrawing:
                                objNewComponentSet.Add(objComponent);
                                break;
                            case Manifold.Interop.ComponentType.ComponentTable:
                                break;
                            default:
                                break;
                        }
                    }

                    //objReturn = p_axManifoldMap.get_Document().NewMap(p_sMapName, objNewComponentSet, p_axManifoldMap.Application.DefaultCoordinateSystem, true);
                    objReturn = p_axManifoldMap.get_Document().NewMap(p_sMapName, objNewComponentSet, p_axManifoldMap.Application.Application.NewCoordinateSystem("Irish Grid"), true);
                    p_axManifoldMap.ComponentName = p_sMapName;
                    p_axManifoldMap.ZoomToFit();
                }
            }
            catch (Exception objEx)
            {
                throw;
            }

            return objReturn;
        }

        public static void setSelection(AxManifold.Interop.AxComponentControl p_MapControl, Manifold.Interop.Drawing p_Drawing, Int32 p_iManifoldObjectID)
        {
            try
            {
                //Clear any Previous selection
                unsetSelection(p_MapControl, p_Drawing);

                //Set selection
                string sSQL = String.Format(@"UPDATE [{0}] SET [Selection (I)] = True WHERE [ID]= {1}", p_Drawing.Name, p_iManifoldObjectID.ToString());
                //Manifold.Interop.Query tempQuery = p_MapControl.get_Document().NewQuery("TempQuery", false);

                //p_MapControl.get_Document().ComponentSet.Remove(tempQuery);
                executeSQL(sSQL);
                p_MapControl.Refresh();
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        public static  void unsetSelection(AxManifold.Interop.AxComponentControl p_MapControl, Manifold.Interop.Drawing p_Drawing)
        {
            try
            {
                string sSQL = string.Format(@"UPDATE [{0}] SET [Selection (I)] = False ", p_Drawing.Name);
                executeSQL(sSQL);
                p_MapControl.Refresh();
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        public static void executeSQL(string sSQL)
        {
            try
            {
                Constants.QUERYDEFAULT.Text = sSQL;
                Constants.QUERYDEFAULT.RunEx(false);
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        public static void setNearestSelection(AxManifold.Interop.AxComponentControl p_MapControl, Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Point p_ptClickedLocation, double p_dblSelectionTolerance)
        {
            try
            {
                executeSQL(getSQL_SetNearestSelection(p_Drawing, p_ptClickedLocation, p_dblSelectionTolerance));
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        private static string getSQL_SetNearestSelection(Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Point p_ptClickedLocation, double p_dblSelectionTolerance)
        {
            StringBuilder sbSQL = new StringBuilder();
            try
            {
                if (p_dblSelectionTolerance < 1)
                {
                    p_dblSelectionTolerance = 50;
                }

                sbSQL.AppendLine(@"UPDATE [" + p_Drawing.Name + "] SET [Selection (I)] = True WHERE ID IN ");
                sbSQL.AppendLine(@"( ");
                sbSQL.AppendLine(@" SELECT TOP 1 [ID]  ");
                sbSQL.AppendLine(@" FROM ");
                sbSQL.AppendLine(@"    [" + p_Drawing.Name + "] AS [tblEdit] ");
                sbSQL.AppendLine(@"    CROSS JOIN ");
                sbSQL.AppendLine(@"    ( ");
                sbSQL.AppendLine(@"    VALUES  ");
                sbSQL.AppendLine(@"        ( ");
                sbSQL.AppendLine(@"      AssignCoordSys(CGeom(CGeomWKB(""POINT (" + p_ptClickedLocation.X.ToString() + " " + p_ptClickedLocation.Y.ToString() + @")"")), CoordSys(""" + p_Drawing.Name + @""" AS COMPONENT)	 ");
                sbSQL.AppendLine(@"        ) ");
                sbSQL.AppendLine(@"    )  ");
                sbSQL.AppendLine(@"    NAMES ([Point]) ");
                sbSQL.AppendLine(@"    ) AS [tblEdit1] ");
                sbSQL.AppendLine(@" WHERE  ");
                sbSQL.AppendLine(@"  Distance([tblEdit].[Geom (I)], [tblEdit1].[Point]) <= " + p_dblSelectionTolerance.ToString() + " ");
                sbSQL.AppendLine(@" ORDER BY Distance([tblEdit].[Geom (I)], [tblEdit1].[Point]) ASC, [Area (I)] ");
                sbSQL.AppendLine(@");");
            }
            catch (Exception objEx)
            {
                throw;
            }
            return sbSQL.ToString();
        }

        public static Int32 getManifoldIDFromQuery(Manifold.Interop.Drawing p_Drawing, string p_sQueryText)
        {
            Int32 iID = -1;
            Manifold.Interop.Query qryQuery = p_Drawing.Document.NewQuery("", false);
            try
            {
                qryQuery.Text = p_sQueryText;
                qryQuery.RunEx(false);

                Manifold.Interop.Table objTableResults = qryQuery.Table;
                if (objTableResults.RecordSet.Count > 0)
                {
                    Manifold.Interop.Record objRecord = (Manifold.Interop.Record)objTableResults.RecordSet[0];
                    string sResult = objRecord.get_DataText("ID");

                    if (string.IsNullOrEmpty(sResult))
                    {
                        iID = 0;
                    }
                    else
                    {
                        iID = Convert.ToInt32(objRecord.get_DataText("ID"));
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                //Remove the query
                p_Drawing.Document.ComponentSet.Remove(qryQuery);
            }
            return iID;
        }

        public static string getBoundingBoxWKT(Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Point p_Point, double p_SelectionTolerance)
        {
            string sReturn = "";
            Manifold.Interop.Query qryQuery = p_Drawing.Document.NewQuery("", false);
            try
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.AppendLine("SELECT CStr(CGeomWKB(");
                sbSQL.AppendLine("AssignCoordSys(BoundingBox(");
                sbSQL.AppendLine("NewLine(");
                sbSQL.AppendLine("    NewPoint(" + p_Point.X.ToString() + " - " + p_SelectionTolerance.ToString() + ", " + p_Point.Y.ToString() + " - " + p_SelectionTolerance.ToString() + "), ");
                sbSQL.AppendLine("    NewPoint(" + p_Point.X.ToString() + " + " + p_SelectionTolerance.ToString() + ", " + p_Point.Y.ToString() + " + " + p_SelectionTolerance.ToString() + "))),");
                sbSQL.AppendLine("CoordSys(\"" + p_Drawing.Name + "\" AS COMPONENT))");
                sbSQL.AppendLine(")) AS WKT FROM [" + p_Drawing.Name + "]");

                qryQuery.Text = sbSQL.ToString();
                qryQuery.RunEx(false);

                Manifold.Interop.Table objTableResults = qryQuery.Table;
                if (objTableResults.RecordSet.Count > 0)
                {
                    Manifold.Interop.Record objRecord = (Manifold.Interop.Record)objTableResults.RecordSet[0];
                    sReturn = objRecord.get_DataText("WKT");
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                //Remove the query
                p_Drawing.Document.ComponentSet.Remove(qryQuery);
            }
            return sReturn;
        }

        public static GeomMovedDetails moveGeomWithSQL(Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Point p_StartPoint, Manifold.Interop.Point p_EndPoint, Int32 p_ManifoldID)
        {
            GeomMovedDetails objReturn = new GeomMovedDetails();
            try
            {
                double dblStartX = p_StartPoint.X;
                double dblEndX = p_EndPoint.X;
                double moveHorizontally = dblEndX - dblStartX;

                double dblStartY = p_StartPoint.Y;
                double dblEndY = p_EndPoint.Y;
                double moveVertically = dblEndY - dblStartY;

                objReturn.StartX = dblStartX;
                objReturn.EndX = dblEndX;
                objReturn.MoveHorizontallyX = moveHorizontally;
                objReturn.StartY = dblStartY;
                objReturn.EndY = dblEndY;
                objReturn.MoveVerticallyY = moveVertically;

                Utility.moveGeomWithSQL(p_Drawing, moveHorizontally, moveVertically, p_ManifoldID);
            }
            catch (Exception err)
            {
                throw;
            }
            return objReturn;
        }

        public static GeomMovedDetails moveGeomWithSQL(Manifold.Interop.Drawing p_Drawing, double p_MoveHorizontallyX, double p_MoveVerticallyY, Int32 p_ManifoldID)
        {
            GeomMovedDetails objReturn = new GeomMovedDetails();
            try
            {
                objReturn.MoveHorizontallyX = p_MoveHorizontallyX;
                objReturn.MoveVerticallyY = p_MoveVerticallyY;
                executeMoveGeomWithSQL(p_Drawing, p_MoveHorizontallyX, p_MoveVerticallyY, p_ManifoldID);
            }
            catch (Exception err)
            {
                throw;
            }
            return objReturn;
        }

        public static void executeMoveGeomWithSQL(Manifold.Interop.Drawing p_Drawing, double p_HorizontalX, double p_VerticalY, Int32 p_ManifoldID)
        {
            try
            {
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.AppendLine("UPDATE [" + p_Drawing.Name + "]");
                sbSQL.AppendLine("SET [Geom (I)] = MoveVertically(MoveHorizontally([Geom (I)]," + p_HorizontalX.ToString() + "), " + p_VerticalY.ToString() + ")");
                sbSQL.AppendLine("WHERE ID = " + p_ManifoldID.ToString());

                Utility.executeSQL(sbSQL.ToString());
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        //NOTE: I think the axManifoldMap.NativeToScreen() method does the same as this.
        public static Manifold.Interop.Point convertPointCoordinates(AxManifold.Interop.AxComponentControl p_Target, Manifold.Interop.Drawing p_Source, Manifold.Interop.Point p_objPoint)
        {
            Manifold.Interop.CoordinateSystem csSource = null;
            Manifold.Interop.CoordinateSystem csTarget = null;
            csTarget = p_Target.Application.DefaultCoordinateSystemLatLon;

            if (p_Source.CoordinateSystemVerified == true)
            {
                csSource = p_Source.CoordinateSystem;
            }
            else
            {
                p_Source.CoordinateSystem = p_Target.Application.Application.NewCoordinateSystem("Irish Grid");
                csSource = p_Target.Application.Application.NewCoordinateSystem("Irish Grid");
                p_Source.CoordinateSystemVerified = true;
            }

            Manifold.Interop.CoordinateConverter objConverter = p_Target.Application.NewCoordinateConverter();
            objConverter.Prepare((Manifold.Interop.Base)csTarget, (Manifold.Interop.Base)csSource);
            objConverter.Convert((Manifold.Interop.Base)p_objPoint, null);
            return p_objPoint;
        }

        public static void DrawPoint(Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Point pt)
        {
            try
            {
                Manifold.Interop.Table objTable = (Manifold.Interop.Table)p_Drawing.OwnedTable;
                StringBuilder sbSQL = new StringBuilder();
                sbSQL.Append("INSERT INTO [" + objTable.Name + "] ([Geom (I)]) VALUES (AssignCoordSys(NewPoint(" + pt.X + ", " + pt.Y + "), CoordSys(\"" + objTable.Name + "\" as COMPONENT)));");

                Manifold.Interop.Query qryQuery = p_Drawing.Document.NewQuery("Temp_AddPoint", false);
                qryQuery.Text = sbSQL.ToString();

                //Commit query
                qryQuery.RunEx(false);

                //Remove the query
                p_Drawing.Document.ComponentSet.Remove(qryQuery);
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        public static void DrawLine(Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Geom p_objGeomLine)
        {
            try
            {
                Manifold.Interop.Table objTable = (Manifold.Interop.Table)p_Drawing.OwnedTable;
                StringBuilder sbSQL = new StringBuilder();


                sbSQL.Append("INSERT INTO [" + objTable.Name + "] ([Geom (I)]) VALUES (AssignCoordSys(CGeom(CGeomWKB(\"" + p_objGeomLine.ToTextWKT() + "\")),CoordSys(\"" + objTable.Name + "\" as COMPONENT)));");
                Manifold.Interop.Query qryQuery = p_Drawing.Document.NewQuery("Temp_AddLine", false);
                qryQuery.Text = sbSQL.ToString();

                //Commit query
                qryQuery.RunEx(false);

                //Remove the query
                p_Drawing.Document.ComponentSet.Remove(qryQuery);
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        public static void DrawArea(Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Geom p_objGeomArea)
        {
            try
            {
                Manifold.Interop.Table objTable = (Manifold.Interop.Table)p_Drawing.OwnedTable;
                StringBuilder sbSQL = new StringBuilder();

                sbSQL.Append("INSERT INTO [" + objTable.Name + "] ([Geom (I)]) VALUES (AssignCoordSys(CGeom(CGeomWKB(\"" + p_objGeomArea.ToTextWKT() + "\")),CoordSys(\"" + objTable.Name + "\" as COMPONENT)));");
                Manifold.Interop.Query qryQuery = p_Drawing.Document.NewQuery("Temp_AddLine", false);
                qryQuery.Text = sbSQL.ToString();

                //Commit query
                qryQuery.RunEx(false);

                //Remove the query
                p_Drawing.Document.ComponentSet.Remove(qryQuery);
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        public static void setSelection(AxManifold.Interop.AxComponentControl p_MapControl, Manifold.Interop.Drawing p_Drawing, Manifold.Interop.Point p_ptClickedLocation, double p_dblSelectionTolerance)
        {
            Manifold.Interop.Query tempQuery = p_MapControl.get_Document().NewQuery("TempQuery", false);
            try
            {
                //Clear any Previous selection
                unsetSelection(p_MapControl, p_Drawing);

                StringBuilder sbSQL = new StringBuilder();
                #region Method1
                //Set current selection
                //sbSQL.AppendLine(@"UPDATE [" + p_Drawing.Name + "] SET [Selection (I)] = True WHERE [ID] IN (");
                //sbSQL.AppendLine(@" SELECT tblDistance.[ID] FROM [" + p_Drawing.Name + "] tblDistance WHERE ");
                //sbSQL.AppendLine(@" DISTANCE(GEOM([ID]),AssignCoordSys(CGeom(CGeomWKB(""POINT (" + p_ptClickedLocation.X.ToString() + " " + p_ptClickedLocation.Y.ToString() + @")"")), CoordSys(""" + p_Drawing.Name + @""" AS COMPONENT))) = ");
                //sbSQL.AppendLine(@" (");
                //sbSQL.AppendLine(@"     SELECT min (DISTANCE(GEOM([ID]),AssignCoordSys(CGeom(CGeomWKB(""POINT (" + p_ptClickedLocation.X.ToString() + " " + p_ptClickedLocation.Y.ToString() + @")"")), CoordSys(""" + p_Drawing.Name + @""" AS COMPONENT))))");
                //sbSQL.AppendLine(@"     FROM [" + p_Drawing.Name + "] tblDistance");
                //sbSQL.AppendLine(@"     WHERE DISTANCE(GEOM([ID]),AssignCoordSys(CGeom(CGeomWKB(""POINT (" + p_ptClickedLocation.X.ToString() + " " + p_ptClickedLocation.Y.ToString() + @")"")), CoordSys(""" + p_Drawing.Name + @""" AS COMPONENT))) < " + p_dblSelectionTolerance);
                //sbSQL.AppendLine(@" ))");
                #endregion

                #region Method2
                //string wkt = String.Format("POINT ({0} {1})", p_ptClickedLocation.X.ToString(), p_ptClickedLocation.Y.ToString());
                //string sql = String.Format(@"UPDATE [{0}] SET [Selection (I)] = True " +
                //                           @"WHERE TOUCHES([ID],BUFFER(AssignCoordSys(CGeom(CGeomWKB(""{1}"")), CoordSys(""{0}"" AS COMPONENT)),{2}))",
                //                           p_Drawing.Name, wkt, p_dblSelectionTolerance);
                #endregion

                //Create selection Rectangle around clicked Point
                Manifold.Interop.PointSet points = p_MapControl.Application.NewPointSet();
                points.Add(p_MapControl.Application.NewPoint(p_ptClickedLocation.X - p_dblSelectionTolerance, p_ptClickedLocation.Y - p_dblSelectionTolerance));
                points.Add(p_MapControl.Application.NewPoint(p_ptClickedLocation.X + p_dblSelectionTolerance, p_ptClickedLocation.Y - p_dblSelectionTolerance));
                points.Add(p_MapControl.Application.NewPoint(p_ptClickedLocation.X + p_dblSelectionTolerance, p_ptClickedLocation.Y + p_dblSelectionTolerance));
                points.Add(p_MapControl.Application.NewPoint(p_ptClickedLocation.X - p_dblSelectionTolerance, p_ptClickedLocation.Y + p_dblSelectionTolerance));
                points.Add(VBManifoldWrapper.ManifoldObjectWrapper.getPointFromPointSet(points, 0));

                //Create the geom to get WKT string to use in search
                Manifold.Interop.Geom geom = p_MapControl.Application.NewGeom(Manifold.Interop.GeomType.GeomArea, null);
                Manifold.Interop.BranchSet geomBranchSet = geom.get_BranchSet();
                VBManifoldWrapper.ManifoldObjectWrapper.setPointSetInBranchSet(points, geomBranchSet);
                string wkt = geom.ToTextWKT();


                sbSQL.Length = 0;
                sbSQL.AppendLine(@"UPDATE [" + p_Drawing.Name + "] SET [Selection (I)] = True ");
                sbSQL.AppendLine("WHERE Touches(AssignCoordSys(CGeom(CGeomWKB(\"" + wkt + "\")), CoordSys(\"" + p_Drawing.Name + "\" AS COMPONENT)), [ID])");

                tempQuery.Text = sbSQL.ToString();
                tempQuery.Run();
                p_MapControl.Refresh();
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                p_MapControl.get_Document().ComponentSet.Remove(tempQuery);
            }
        }
    }


}
