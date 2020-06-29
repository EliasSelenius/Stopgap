using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Glow;
using Nums;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Stopgap {

    public struct Particle {
        public vec3 pos;
        public vec2 scale;
        public float rotation;

        public vec3 velocity;

        public vec4 color;

        public float startTime;
        public float elapsedTime => Game.time - startTime;
    }

    public class ParticleSystem : Component, IRenderable {

        private static readonly Mesh quad;

        static ParticleSystem() {
            quad = Mesh.GenQuad(); quad.Init();
        }

        public Material material;
        private readonly List<Particle> particles = new List<Particle>();
        public float spawnRate;
        public float lifetime;

        public Func<vec3> startPos = null;
        public Func<vec2> startScale = null;
        public Func<float> startRotation = null;
        public Func<vec3> startVelocity = null;
        public Func<vec4> startColor = null;

        public Func<Particle> newParticle = null;

        public ParticleSystem(Material material, float spawnRate, float lifetime) {
            this.material = material;
            this.spawnRate = spawnRate;
            this.lifetime = lifetime;

        }

        protected override void OnEnter() => scene.renderables.Add(this);
        protected override void OnLeave() => scene.renderables.Remove(this);

        protected override void Update() {
            for (int i = 0; i < particles.Count; i++) {
                var p = particles[i];
                p.pos += p.velocity * Game.deltaTime;
                particles[i] = p;
                if (p.elapsedTime > lifetime) { 
                    particles.RemoveAt(i);
                }
            }

            Emit(spawnRate * Game.deltaTime);
        }

        private float queue = 0;
        public void Emit(float count) {
            queue += count;
            var s = math.floor(queue);
            for (int i = 0; i < s; i++) {
                particles.Add(getNewParticle());
            }
            queue -= s;
        }

        private Particle getNewParticle() {
            var p = newParticle?.Invoke() ?? new Particle();

            p.pos = gameObject.transform.position + (startPos?.Invoke() ?? p.pos);
            p.scale = (startScale?.Invoke() ?? p.scale);
            p.rotation = (startRotation?.Invoke() ?? p.rotation);
            p.velocity = (startVelocity?.Invoke() ?? p.velocity);
            p.color = (startColor?.Invoke() ?? p.color);
            p.startTime = Game.time;

            return p;
        }


        public void render() {

            material.apply();
            for (int i = 0; i < particles.Count; i++) {
                var particle = particles[i];
                var m = Matrix4.CreateTranslation(particle.pos.ToOpenTKVec());
                var v = Camera.MainCamera.view_matrix;

                m.M11 = v.M11;
                m.M12 = v.M21;
                m.M13 = v.M31;
                m.M21 = v.M12;
                m.M22 = v.M22;
                m.M23 = v.M32;
                m.M31 = v.M13;
                m.M32 = v.M23;
                m.M33 = v.M33;

                m = Matrix4.CreateScale(new Vector3(particle.scale.x, particle.scale.y, 1)) * Matrix4.CreateRotationZ(particle.rotation) * m;

                material.shader.set_vec4("tint", particle.color);
                material.shader.set_mat4("model", m);
                quad.Render();
            }
        }
    }
}
