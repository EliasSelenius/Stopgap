using System;
using System.Collections.Generic;
using System.Linq;

using Stopgap;
using Nums;
using Stopgap.Gui;

public class TestCounter : Component {

    int count = 0;
    Textbox text;

    public override void Start() {
        text = Game.canvas.Create<Textbox>();
        text.anchor = Anchor.top_right;
        text.draw_background = false;
        text.font_size = 0.5f;


        gameObject.addComponents(new MeshRenderer(Assets.getMesh("sphere"), PBRMaterial.Default), new SphereCollider());
    }

    protected override void Update() {
        if (Input.LeftMousePressed) {
            if (scene.raycast(scene.main_camera.transform.position, scene.main_camera.screenToRay(Input.MousePos_ndc)) == gameObject) {
                count++;
                text.SetText(count.ToString());
            }
        }
    }

}