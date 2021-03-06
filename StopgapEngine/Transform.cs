﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using Nums;
using JsonParser;
using System.Collections.Specialized;
using System.Xml;

namespace Stopgap {
    public class Transform {

        public static Transform FromJson(JObject json) {
            var res = new Transform();

            if (json.ContainsKey("position")) {
                var pos = json["position"] as JArray;
                res.position = new vec3(pos[0] as JNumber, pos[1] as JNumber, pos[2] as JNumber);
            }

            if (json.ContainsKey("scale")) {
                var s = json["scale"] as JArray;
                res.scale = new vec3(s[0] as JNumber, s[1] as JNumber, s[2] as JNumber);
            }

            if (json.ContainsKey("rotation")) {
                var r = json["rotation"] as JArray;
                res.rotation = new Quaternion(r[0] as JNumber, r[1] as JNumber, r[2] as JNumber, r[3] as JNumber);
            } // todo: euler, axis angle support here

            return res;
        }

        public void setFromXml(XmlElement xml) {
            XmlElement el;
            if (xml.getChild("matrix", out el)) {
                // TODO: implement
            } else {
                if (xml.getChild("position", out el)) position = el.asVec3();
                if (xml.getChild("scale", out el)) scale = el.asVec3();
                if (xml.getChild("rotation", out el)) rotation = el.asVec4().toOpenTKQuat();
                else if (xml.getChild("euler", out el)) rotation = Quaternion.FromEulerAngles(el.asVec3().ToOpenTKVec());
                else if (xml.getChild("axis_angle", out el)) {
                    var v = el.asVec4();
                    rotation = Quaternion.FromAxisAngle(v.xyz.ToOpenTKVec(), v.w);
                } 
            }
        }


        public Matrix4 matrix {
            get => Matrix4.CreateScale(scale.ToOpenTKVec()) * Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(position.ToOpenTKVec());
            set {
                position = value.ExtractTranslation().ToNumsVec();
                scale = value.ExtractScale().ToNumsVec();
                rotation = value.ExtractRotation();
            }
        }

        public vec3 forward => matrix.Row2.Xyz.ToNumsVec();
        public vec3 back => -forward;
        public vec3 right => matrix.Row0.Xyz.ToNumsVec();
        public vec3 left => -right;
        public vec3 up => matrix.Row1.Xyz.ToNumsVec();
        public vec3 down => -up;

        public vec3 position = vec3.zero;
        public vec3 scale = vec3.one;
        public Quaternion rotation = Quaternion.Identity;

        #region Constructors

        public Transform() { }
        public Transform(vec3 pos) => position = pos;
        public Transform(vec3 pos, vec3 scl) {
            position = pos; scale = scl;
        }
        public Transform(Quaternion rot) => rotation = rot;
        public Transform(vec3 pos, Quaternion rot) {
            position = pos; rotation = rot;
        }
        public Transform(vec3 pos, vec3 scl, Quaternion rot) {
            position = pos; scale = scl; rotation = rot;
        }
        #endregion

        public void set(Transform transform) {
            position = transform.position;
            scale = transform.scale;
            rotation = transform.rotation;
        }

        public void set(vec3 pos, vec3 scale, Quaternion rot) {
            position = pos;
            this.scale = scale;
            rotation = rot;
        }

        public void Translate(vec3 v) {
            position += v;
        }

        public void Translate(float x, float y, float z) {
            position.x += x;
            position.y += y;
            position.z += z;
        }

        public void Rotate(Quaternion q) {
            rotation *= q;
        }

        public void Rotate(vec3 axis, float angle) {
            rotation = Quaternion.FromAxisAngle(axis.ToOpenTKVec(), angle) * rotation;
        }

        public void Rotate(vec3 euler) {
            rotation = Quaternion.FromEulerAngles(euler.ToOpenTKVec()) * rotation;
        }

        public void setRotation(vec3 euler) => rotation = Quaternion.FromEulerAngles(euler.ToOpenTKVec());
        public void setRotation(vec3 axis, float angle) => rotation = Quaternion.FromAxisAngle(axis.ToOpenTKVec(), angle);
        public void setRotation(Quaternion quat) => rotation = quat;

        public void LookIn(vec3 dir) => LookIn(dir, vec3.unity);
        public void LookIn(vec3 dir, vec3 up) {
            var m = Matrix4.LookAt(position.ToOpenTKVec(), (position + dir).ToOpenTKVec(), up.ToOpenTKVec()).Inverted();
            m.Row0.Xyz = -m.Row0.Xyz;
            m.Row2.Xyz = -m.Row2.Xyz;
            rotation = m.ExtractRotation();
        }

        public void LookAt(vec3 point) => LookAt(point, vec3.unity);
        public void LookAt(vec3 point, vec3 up) {

            var m = Matrix4.LookAt(position.ToOpenTKVec(), point.ToOpenTKVec(), up.ToOpenTKVec()).Inverted();
            m.Row0.Xyz = -m.Row0.Xyz;
            m.Row2.Xyz = -m.Row2.Xyz;
            rotation = m.ExtractRotation();
        }

        public void RotateAround(vec3 point, vec3 axis, float angle) => RotateAround(point, Quaternion.FromAxisAngle(axis.ToOpenTKVec(), angle));
        public void RotateAround(vec3 point, vec3 euler) => RotateAround(point, Quaternion.FromEulerAngles(euler.ToOpenTKVec()));
        public void RotateAround(vec3 point, Quaternion rot) {
            var v = position - point;
            var r = RotateVector(rot, v);
            position = point + r;
        }

        public static vec3 RotateVector(Quaternion rot, vec3 v) {
            var qv = new Quaternion(v.ToOpenTKVec(), 0);
            var c = rot;
            c.Conjugate();
            return (rot * qv * c).Xyz.ToNumsVec();
        }

    }
}
