using Nums;
using OpenTK;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Stopgap {
    public static class Utils {
        public static bool getChild(this XmlElement xml, string name, out XmlElement child) => (child = xml[name]) != null;
        private static float[] getFloats(string str) => str.Split(' ').Select(x => float.Parse(x)).ToArray();

        public static vec3 parseVec3(string str) {
            var fs = getFloats(str);
            if (fs.Length == 1) return new vec3(fs[0], fs[0], fs[0]);
            return new vec3(fs[0], fs[1], fs[2]);
        }
        public static vec4 parseVec4(string str) {
            var fs = getFloats(str);
            if (fs.Length == 1) return new vec4(fs[0], fs[0], fs[0], fs[0]);
            return new vec4(fs[0], fs[1], fs[2], fs[3]);
        }

        public static vec3 asVec3(this XmlElement xml) => parseVec3(xml.InnerText);
        public static vec4 asVec4(this XmlElement xml) => parseVec4(xml.InnerText);

        public static Quaternion toOpenTKQuat(this vec4 v) => new Quaternion(v.x, v.y, v.z, v.w);

        private static Dictionary<string, Func<string, object>> xmlValueGetters = new Dictionary<string, Func<string, object>>() {
            { "string", s => s },
            { "float", s => float.Parse(s) },
            { "vec3", s => parseVec3(s) },
            { "vec4", s => parseVec4(s) },
            { "Mesh", s => Assets.getMesh(s) },
            { "Material", s => Assets.getMaterial(s) }
        };
        internal static object getValueFromXml(XmlElement xml) {
            var type = xml.GetAttribute("type");
            return xmlValueGetters[type](xml.InnerText);
        }

        #region matrix manipulation
        private static List<mat4> matrices = new List<mat4>();
        public static mat4 matrix { 
            get => matrices[matrices.Count - 1];
            set => matrices[matrices.Count - 1] = value;
        } 
        public static void pushMatrix() {
            if (matrices.Count == 0) matrices.Add(mat4.identity);
            else matrices.Add(matrix);   
        }
        public static mat4 popMatrix() {
            var m = matrix;
            matrices.RemoveAt(matrices.Count - 1);
            return m;
        }

        public static void translate(vec3 translation) {
            var m = matrix;
            m.m14 += translation.x;
            m.m24 += translation.y;
            m.m34 += translation.z;
            matrix = m;
        }

        #endregion
    }
}
