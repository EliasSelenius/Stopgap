using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Stopgap.Gui;
using Nums;

namespace Stopgap.Editor {
    class Program {
        static void Main(string[] args) {


            Game.onLoad = () => {
                
                Game.SetScene(new Scene());

                var user = new GameObject();
                user.AddComps(
                    new Camera(),
                    new CamFlyController());
                user.EnterScene(Game.scene);

                var testObj = new GameObject();
                testObj.AddComps(
                    new MeshRenderer(Assets.GetMesh("SpaceShip"), Material.Chrome));

                var m = testObj.GetComponent<MeshRenderer>().mesh;
                m.Mutate(v => {
                    v.pos += new Nums.vec3(Nums.Noise.Random(), Nums.Noise.Random(), Nums.Noise.Random());
                    return v;
                });
                //m.Apply();

                testObj.EnterScene(Game.scene);

                var quadObj = new GameObject();
                quadObj.AddComp(new MeshRenderer(Mesh.GenQuad(), Material.Turquoise));
                //quadObj.EnterScene(Game.scene);
                quadObj.transform.scale = (1000, 1000, 1000);
                quadObj.transform.Rotate((-(float)Math.PI / 2f, 0f, 0f));
                
                var galleon = new GameObject();
                galleon.transform.position = (80, 0, 0);
                galleon.transform.scale = (2, 2, 2);
                galleon.AddComp(new MeshRenderer(Assets.GetMesh("GalleonBoat"), Material.Ruby));
                galleon.EnterScene(Game.scene);


                // init lots of objects

                var icosphere = Mesh.GenIcosphere();
                icosphere.Subdivide(2);
                icosphere.Mutate(v => {
                    var n = v.pos.normalized;
                    v.pos = n + n * .1f * Noise.Random((int)(v.pos.x*10f), (int)((v.pos.y + v.pos.z)*10f));
                    return v;
                });
                icosphere.GenNormals();

                for (int i = 0; i < 900; i += 3) {
                    var g = new GameObject();
                    g.AddComp(new MeshRenderer(icosphere, Material.Jade));
                    g.transform.position = new Nums.vec3(Nums.Noise.Random(i),
                                                         Nums.Noise.Random(i + 1),
                                                         Nums.Noise.Random(i + 2)) * 500;
                    g.transform.scale *= Noise.Random(i)*7 + 20;
                    g.EnterScene(Game.scene);
                }


                var rigidbody = new GameObject();
                var r = new Rigidbody();
                const float pi = (float)Math.PI;
                rigidbody.AddComps(r, new MeshRenderer(Assets.GetMesh("GalleonBoat"), Material.Greenglow));
                rigidbody.EnterScene(Game.scene);
                rigidbody.transform.position.y = 20;

                r.AddForce(vec3.unitx, vec3.one);


                // generate planet
                var planet = new GameObject();
                var pmesh = Mesh.GenIcosphere();
                pmesh.Subdivide(3);
                pmesh.Mutate(v => {
                    var n = v.pos.normalized;
                    v.pos = n + n * .1f * Noise.Random((int)(v.pos.x * 10f), (int)((v.pos.y + v.pos.z) * 10f));
                    return v;
                });
                pmesh.GenNormals();
                planet.AddComps(new MeshRenderer(pmesh, Material.Silver));
                planet.EnterScene(Game.scene);
                planet.transform.position = (0, 0, 500);
                planet.transform.scale *= 100;
                pmesh.Apply();


                var billboard = new GameObject();
                billboard.AddComp(new Billboard(Material.RedPlastic));
                billboard.EnterScene(Game.scene);
                billboard.transform.position = (0, 40, 0);
                billboard.transform.scale *= 3;

                var ps = new GameObject();
                ps.AddComp(new ParticleSystem(Material.Greenglow, 40, 20) { 
                    startVelocity = () => MyMath.RandomDirection((int)(Game.time * 1000f)) * Noise.Random((int)(Game.time * 1000f) + 3) * 3f,
                });
                ps.transform.position = (-40, 20, 0);
                ps.EnterScene(Game.scene);

                Renderer.renderNormals = false;
                // init editor Gui
                
                InitGUI();    

                
            };

            Game.Run();
        }

        private static void InitGUI() {
            var c = Game.canvas = new Gui.Canvas();

            /*var test = c.Create<TextBox>();
            test.editable = true;
            test.AppendText("Hello World");
            */

            var cmdLine = c.Create<TextBox>();
            cmdLine.background_color = (.3f, .3f, .3f, 1);
            cmdLine.font_size = 6;
            cmdLine.editable = true;
            cmdLine.size.y = 0.08f;
            cmdLine.size.x = 3;
            cmdLine.pos.y = -0.96f;
            var vars = new Dictionary<string, GameObject>();
            cmdLine.OnInput += (t, e) => {
                if (e.Key == OpenTK.Input.Key.Enter) {
                    var text = t.text;
                    t.RemoveText(0, text.Length);
                    Console.WriteLine(text);
                    var g = new GameObject();
                    g.AddComps(new MeshRenderer(Assets.GetMesh("sphere"), Material.Default));
                    g.EnterScene(Game.scene);
                    vars[text] = g;
                }
            };

            // FPS display:
            var fpsd = c.Create<TextBox>();
            fpsd.font_size = 6;
            fpsd.size.y = 0.05f;
            fpsd.size.x = 0.4f;
            fpsd.pos.y = 0.95f;
            fpsd.pos.x = -1f;
            fpsd.draw_background = false;
            fpsd.OnUpdate += e => {
                fpsd.SetText("FPS: " + Math.Round(Game.window.RenderFrequency));
            };

        }
    }
}
  