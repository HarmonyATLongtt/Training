using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_Sample.Utils
{
    internal static class SolidUtils
    {
        #region Solids

        /// <summary>
        ///  Get the union solid from given element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elem"></param>
        /// <param name="getInstGeo"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Solid GetTotalSolid(Document doc,
                                          Element elem,
                                          bool getInstGeo,
                                          View view = null)
        {
            List<Solid> solids = GetAllSolids(doc, elem, getInstGeo, view);
            Solid total = null;
            foreach (Solid solid in solids)
            {
                if (null == total)
                    total = solid;
                else
                    total = BooleanOperationsUtils.ExecuteBooleanOperation(total, solid, BooleanOperationsType.Union);
            }
            return total;
        }

        /// <summary>
        /// get all solids of a given element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elem"></param>
        /// <param name="getInsGeo"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static List<Solid> GetAllSolids(Document doc,
                                                Element elem,
                                                bool getInsGeo,
                                                View view = null)
        {
            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false
            };
            if (view != null)
                options.View = view;

            GeometryElement geoElem = elem.get_Geometry(options);
            List<Solid> solids = new List<Solid>();
            GetSolidFromGeometry(doc, geoElem, getInsGeo, ref solids);
            return solids;
        }

        /// <summary>
        /// recursively get solid from geometry element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="geoElem"></param>
        /// <param name="getInstGeo"></param>
        /// <param name="solids"></param>
        /// <param name="view"></param>
        public static void GetSolidFromGeometry(Document doc,
                                                GeometryElement geoElem,
                                                bool getInstGeo,
                                                ref List<Solid> solids,
                                                View view = null)
        {
            foreach (GeometryObject geoObj in geoElem)
            {
                if (geoObj is Solid solid
                    && solid.Volume > 0
                    && IsSolidGraphicallyVisible(doc, view, solid))
                    solids.Add(solid);
                else if (geoObj is GeometryInstance geoInst)
                {
                    GeometryElement innerGeo = getInstGeo ? geoInst.GetInstanceGeometry() : geoInst.GetSymbolGeometry();
                    GetSolidFromGeometry(doc, innerGeo, getInstGeo, ref solids, view);
                }
            }
        }

        /// <summary>
        /// check solid is visible in view given
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="view"></param>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static bool IsSolidGraphicallyVisible(Document doc, View view, Solid solid)
        {
            if (doc != null
                && view != null
                && solid.GraphicsStyleId != null
                && solid.GraphicsStyleId != ElementId.InvalidElementId)
            {
                if (doc.GetElement(solid.GraphicsStyleId) is GraphicsStyle graphicalStyle
                    && graphicalStyle.GraphicsStyleCategory != null)
                    return graphicalStyle.GraphicsStyleCategory.get_Visible(view);
            }
            return true;
        }

        /// <summary>
        /// Get solid of element
        /// </summary>
        /// <param name="OrgElm"></param>
        /// <returns></returns>
        public static List<Solid> GetSolidsFromElement(Element OrgElm)
        {
            var opt1 = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Coarse
            };
            var geoElem = OrgElm.get_Geometry(opt1);

            return GetSolidFromGeometryElement(geoElem);
        }

        /// <summary>
        /// get solid from geomety of element
        /// </summary>
        /// <param name="geomElm"></param>
        /// <returns></returns>
        public static List<Solid> GetSolidFromGeometryElement(GeometryElement geomElm)
        {
            var lstSolids = new List<Solid>();
            foreach (GeometryObject geoObj in geomElm)
            {
                if (geoObj is GeometryInstance geoInst)
                {
                    GeometryElement geoElmInst = geoInst.GetSymbolGeometry();
                    lstSolids.AddRange(GetSolidFromGeometryElement(geoElmInst));
                }
                else if (geoObj is Solid geoSolid)
                {
                    lstSolids.Add(geoSolid);
                }
            }

            return lstSolids;
        }

        #endregion Solids
    }
}