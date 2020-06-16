﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Stopgap.Gui;
using Nums;
using OpenTK;
using System.Diagnostics.PerformanceData;

namespace Stopgap.Editor {
    class Program {
        static void Main(string[] args) {


            Game.onLoad = () => {
                
                Game.SetScene(new Scene());


                // test collada import:
                var xml = new System.Xml.XmlDocument();
                xml.Load("data/models/Ships2.dae");
                //var collada = new Collada(xml);
                //collada.ToGameObject();


                var user = new GameObject();
                user.AddComps(
                    new Camera(),
                    new CamFlyController());
                user.EnterScene(Game.scene);

                var testObj = new GameObject();
                testObj.AddComps(
                    new MeshRenderer(Assets.GetMesh("SpaceShip"), BlinnPhongMaterial.Chrome));

                var m = testObj.GetComponent<MeshRenderer>().mesh;
                m.Mutate(v => {
                    v.pos += new Nums.vec3(math.rand(), math.rand(), math.rand());
                    return v;
                });
                //m.Apply();

                //testObj.EnterScene(Game.scene);

                var quadObj = new GameObject();
                quadObj.AddComp(new MeshRenderer(Mesh.GenQuad(), BlinnPhongMaterial.Turquoise));
                //quadObj.EnterScene(Game.scene);
                quadObj.transform.scale = (1000, 1000, 1000);
                quadObj.transform.Rotate((-(float)Math.PI / 2f, 0f, 0f));
                
                var galleon = new GameObject();
                galleon.transform.position = (80, 0, 0);
                galleon.transform.scale = (2, 2, 2);
                galleon.AddComp(new MeshRenderer(Assets.GetMesh("GalleonBoat"), BlinnPhongMaterial.Ruby));
                galleon.EnterScene(Game.scene);


                // init lots of objects

                var icosphere = Mesh.GenIcosphere();
                icosphere.Subdivide(2);
                icosphere.Mutate(v => {
                    var n = v.pos.normalized();
                    v.pos = n + n * .1f * math.rand((int)(v.pos.x*10f), (int)((v.pos.y + v.pos.z)*10f));
                    return v;
                });
                icosphere.GenNormals();


                for (int i = 0; i < 200; i += 3) {
                    var g = new GameObject();
                    g.AddComps(new MeshRenderer(icosphere, new PBRMaterial { 
                        albedo = vec3.one * .8f,
                        metallic = math.range(0, 1),
                        roughness = math.range(0, 1)
                    }), new ParticleSystem(BlinnPhongMaterial.Greenglow, 20, 10) {
                        startVelocity = () => MyMath.RandomDirection((int)(Game.time * 1000f)) * math.rand((int)(Game.time * 1000f) + 3) * 3f,
                    });
                    g.transform.position = new Nums.vec3(math.rand(i),
                                                         math.rand(i + 1),
                                                         math.rand(i + 2)) * 500;
                    g.AddComps(new Rigidbody { Mass = 10f, Velocity = (30, 0, 0) }, new GravitationalObject(), new SphereCollider());
                    g.transform.scale *= math.rand(i)*7 + 20;
                    g.EnterScene(Game.scene);
                }


                {
                    const int count = 10;
                    for (int x = 1; x <= count; x++) {
                        for (int y = 1; y <= count; y++) {
                            var g = new GameObject(new MeshRenderer(Assets.GetMesh("sphere"), new PBRMaterial {
                                albedo = (1, 1, 1),
                                metallic = (float)x / count,
                                roughness = (float)y / count
                            }));
                            g.transform.position += (x, y, 0f);
                            g.transform.position *= 2f;
                            g.EnterScene(Game.scene);
                        }
                    }
                }


                var rigidbody = new GameObject();
                var r = new Rigidbody();
                
                rigidbody.AddComps(r, new MeshRenderer(Assets.GetMesh("GalleonBoat"), BlinnPhongMaterial.Greenglow));
                //rigidbody.EnterScene(Game.scene);
                rigidbody.transform.position.y = 20;

                r.AddForce(vec3.unitx, vec3.one);


                // generate planet
                var planet = new GameObject();
                var pmesh = Mesh.GenIcosphere();
                pmesh.Subdivide(3);
                pmesh.Mutate(v => {
                    var n = v.pos.normalized();
                    v.pos = n + n * .1f * math.gradnoise(v.pos * 10f);
                    return v;
                });
                pmesh.GenNormals();
                planet.AddComps(new MeshRenderer(pmesh, BlinnPhongMaterial.Silver), new Rigidbody { Mass = 10000f }, new GravitationalObject());
                planet.EnterScene(Game.scene);
                planet.transform.position = (0, 0, 700);
                planet.transform.scale *= 100;
                pmesh.Apply();


                var billboard = new GameObject();
                billboard.AddComps(new Billboard(BlinnPhongMaterial.RedPlastic), new Test());
                billboard.EnterScene(Game.scene);
                billboard.transform.position = (0, 40, 0);
                billboard.transform.scale *= 3;

                var ps = new GameObject();
                ps.AddComp(new ParticleSystem(BlinnPhongMaterial.Greenglow, 40, 20) { 
                    startVelocity = () => MyMath.RandomDirection((int)(Game.time * 1000f)) * math.rand((int)(Game.time * 1000f) + 3) * 3f,
                });
                ps.transform.position = (-40, 20, 0);
                ps.EnterScene(Game.scene);

                //Game.renderer.renderNormals = false;
                // init editor Gui
                

                InitGUI();    

                
            };

            Game.Run();
        }

        class Test : Component {
            protected override void Update() {
                transform.position = Camera.MainCamera.screenToRay(Input.MousePos_ndc);
            }
        }

        private static void InitGUI() {
            var c = Game.canvas = new Gui.Canvas(Game.window.Width, Game.window.Height);

            /*var test = c.Create<TextBox>();
            test.editable = true;
            test.AppendText("Hello World");
            */

            var cmdLine = c.Create<TextBox>();
            cmdLine.background_color = (.3f, .3f, .3f, 1);
            cmdLine.font_size = .3f;
            cmdLine.editable = true;
            cmdLine.size.y = unit.parse("0.05vh");
            cmdLine.size.x = unit.parse("0.9vw");
            cmdLine.pos.y = unit.parse("-0.4vh");
            var vars = new Dictionary<string, GameObject>();
            cmdLine.OnInput += (t, e) => {
                if (e.Key == OpenTK.Input.Key.Enter) {
                    var text = t.text;
                    t.RemoveText(0, text.Length);
                    Console.WriteLine(text);
                    var g = new GameObject();
                    g.AddComps(new MeshRenderer(Assets.GetMesh("sphere"), BlinnPhongMaterial.Default));
                    g.EnterScene(Game.scene);
                    vars[text] = g;
                }
            };
        }
    }
}
  