using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FirstCommand.Support.GenericClass.ComparerUtils;

namespace FirstCommand.Support.TrianglesHandle
{
    public class TriangleVertexMap
    {
        #region Properties

        private readonly Dictionary<XYZ, List<MeshTriangle>> _vertexToTriangles;

        public Dictionary<XYZ, List<MeshTriangle>> VertexToTriangles
        {
            get { return _vertexToTriangles; }
        }

        private readonly Dictionary<MeshTriangle, TriangleInfos> _triangleToInfo;
        private readonly HashSet<XYZ> _allVertices;

        #endregion Properties

        #region Contructor

        public TriangleVertexMap(IEnumerable<MeshTriangle> meshTriangles)
        {
            _vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(Comparers.XYZ);
            _triangleToInfo = new Dictionary<MeshTriangle, TriangleInfos>();
            _allVertices = new HashSet<XYZ>(Comparers.XYZ);
            foreach (var meshTri in meshTriangles)
            {
                var triInfo = new TriangleInfos(meshTri); // sử dụng constructor

                _triangleToInfo[meshTri] = triInfo;

                foreach (var pt in triInfo.Vertexes())
                {
                    _allVertices.Add(pt);
                    if (!_vertexToTriangles.TryGetValue(pt, out var list))
                    {
                        list = new List<MeshTriangle>();
                        _vertexToTriangles[pt] = list;
                    }
                    list.Add(meshTri);
                }
            }
        }

        #endregion Contructor

        #region Method

        /// <summary>
        /// Lấy ra tất cả vertex trong tập hợp ban đầu
        /// </summary>
        public List<XYZ> GetAllVertices()
        {
            return _allVertices.ToList();
        }

        /// <summary>
        /// Lấy danh sách triangle có chứa điểm này
        /// </summary>
        public List<MeshTriangle> GetTriangles(XYZ vertex)
        {
            return _vertexToTriangles.TryGetValue(vertex, out var list) ? list : new List<MeshTriangle>();
        }

        /// <summary>
        /// Lấy ra tất cả vertex từ 1 tam giác (dựa vào dữ liệu cache ban đầu)
        /// </summary>
        public List<XYZ> GetVerticesOfTriangle(MeshTriangle triangle)
        {
            return _triangleToInfo.TryGetValue(triangle, out var triInfo) ? triInfo.Vertexes() : new List<XYZ>();
        }

        /// <summary>
        /// Lấy ra tất cả vertex từ 1 danh sách tam giác (dựa vào dữ liệu cache ban đầu)
        /// </summary>
        public List<XYZ> GetVerticesOfGroupTriangles(IEnumerable<MeshTriangle> triangles)
        {
            var result = new HashSet<XYZ>(Comparers.XYZ);

            foreach (var tri in triangles)
            {
                if (_triangleToInfo.TryGetValue(tri, out var info))
                {
                    result.UnionWith(info.Vertexes());
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// Lấy TriangleInfos từ MeshTriangle
        /// </summary>
        public TriangleInfos GetTriangleInfos(MeshTriangle triangle)
        {
            return _triangleToInfo.TryGetValue(triangle, out var info) ? info : null;
        }

        #endregion Method
    }
}