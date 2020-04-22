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

        public float startTime;
        public float elapsedTime => Game.time - startTime;
    }

    public class ParticleSystem : Component, Renderer.IRenderable {

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

        

        public ParticleSystem(Material material, float spawnRate, float lifetime) {
            this.material = material;
            this.spawnRate = spawnRate;
            this.lifetime = lifetime;
        }

        public override void OnEnter() => Renderer.SetObject(gameObject.scene, Renderer.defaultShader, this);
        public override void OnLeave() => Renderer.RemoveObject(gameObject.scene, Renderer.defaultShader, this);
        
        public override void Update() {
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
            return new Particle {
                pos = gameObject.transform.position + (startPos?.Invoke() ?? vec3.zero),
                scale = (startScale?.Invoke() ?? vec2.one),
                rotation = (startRotation?.Invoke() ?? 0),
                velocity = (startVelocity?.Invoke() ?? vec3.zero),
                startTime = Game.time
            };
        }


        public void Render(ShaderProgram shader) {

            material.Apply(shader);
            for (int i = 0; i < particles.Count; i++) {
                var particle = particles[i];
                var m = Matrix4.CreateTranslation(particle.pos.ToOpenTKVec());
                var v = Camera.MainCamera.viewMatrix;

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

                shader.SetMat4("obj_transform", m);
                quad.Render();
            }
        }
    }
}
