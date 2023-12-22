﻿using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using OmsiHook;

namespace OmsiExtensionsCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("#=#=#=#=#=# OmsiExtensions Testing #=#=#=#=#=#");

            OmsiHook.OmsiHook omsi = new();
            omsi.AttachToOMSI().Wait();
            Console.Clear();
            DXTests dXTests = new();
            dXTests.Init(omsi);
            bool toggle = false;
            var playerVehicle = omsi.Globals.PlayerVehicle;
            var progMan = omsi.Globals.ProgamManager;
            var meshes = playerVehicle?.ComplObjInst?.ComplObj?.Meshes;
            var meshInsts = playerVehicle?.ComplObjInst?.AnimSubMeshInsts;
            while (true)
            {
                playerVehicle ??= omsi.Globals.PlayerVehicle;
                var pos = playerVehicle?.Position ?? default;
                var posa = playerVehicle?.AbsPosition ?? default;
                var vel = playerVehicle?.Velocity ?? default;
                var map = omsi.Globals.Map;
                var cam = omsi.Globals.Camera;
                var camPos = cam?.Pos ?? default;
                var weather = omsi.Globals.Weather;
                var tickets = omsi.Globals.TicketPack;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine(($"Read data: x:{pos.x:F3}   y:{pos.y:F3}   z:{pos.z:F3}      " +
                    $"tile:{playerVehicle?.Kachel??0}").PadRight(Console.WindowWidth - 1));
                Console.WriteLine($"Read data: vx:{vel.x:F3}   vy:{vel.y:F3}   vz:{vel.z:F3}".PadRight(Console.WindowWidth-1));
                Console.WriteLine($"Read data: ax:{posa._30:F3}   ay:{posa._31:F3}   az:{posa._32:F3}".PadRight(Console.WindowWidth-1));

                Console.WriteLine($"Read data: map:{map?.Name}   path:{map?.Filename}   friendly:{map?.FriendlyName}".PadRight(Console.WindowWidth-1));
                Console.WriteLine($"{omsi.Globals.Time.Day}/{omsi.Globals.Time.Month}/{omsi.Globals.Time.Year} - {omsi.Globals.Time.Hour}:{omsi.Globals.Time.Minute}:{omsi.Globals.Time.Second:F2}");
                Console.WriteLine($"Camera data: x:{camPos.x:F3}   y:{camPos.y:F3}   z:{camPos.z:F3}      ".PadRight(Console.WindowWidth - 1));
                Console.WriteLine($"{omsi.Globals.Drivers}".PadRight(Console.WindowWidth - 1));

                /*if(!dXTests.IsReady)
                    dXTests.CreateTexture();
                if(dXTests.IsReady)
                    dXTests.UpdateTexture();*/

                progMan ??= omsi.Globals.ProgamManager;
                meshes ??= playerVehicle?.ComplObjInst?.ComplObj?.Meshes;
                meshInsts ??= playerVehicle?.ComplObjInst?.AnimSubMeshInsts;

                Console.WriteLine($"[MOUSE] pos: {progMan.MausPos} ray_pos: {progMan.MausLine3DPos} ray_dir: {progMan.MausLine3DDir}".PadRight(Console.WindowWidth - 1));
                Console.WriteLine($"[MOUSE] {progMan.MausCrossObjFlat} {progMan.MausCrossObjFlat_ObjHeight} {progMan.Maus_MeshEvent}".PadRight(Console.WindowWidth - 1));
                if (meshes != null)
                {
                    int i = 0;
                    foreach (var mesh in meshes)
                    {
                        if(mesh.MausEvent != null)
                        {
                            Console.WriteLine($"  Mesh '{mesh.Filename_Int}' has event: {mesh.MausEvent}");
                            i++;
                        }
                        if(i > 20)
                        {
                            Console.WriteLine("  ...");
                            break;
                        }
                    }
                }
                OmsiAnimSubMesh clickMesh = null;
                OmsiAnimSubMeshInst clickMeshInst = null;
                string mouseEventName = "INEO_Click";
                if(meshes != null)
                    clickMesh = meshes.FirstOrDefault(mesh => mesh.MausEvent == mouseEventName);
                if (clickMesh != null)
                    clickMeshInst = meshInsts[meshes.IndexOf(clickMesh)];
                if (clickMesh != null && progMan.Maus_MeshEvent == mouseEventName && (progMan.Maus_Clicked)) 
                {
                    // Work out object space coords of the mouse click
                    // Note that (if wrapped) we could use D3DXIntersect to find the UV coords given the submesh's d3d mesh

                    // Transform the mouse ray from world space to object space
                    Matrix4x4 invMat;
                    //if (clickMesh.UseCharMatrix)
                    //    invMat = clickMesh.CharMatrixInv;
                    //else
                    Matrix4x4.Invert((Matrix4x4)clickMeshInst.Matrix, out invMat);
                    Matrix4x4.Invert(clickMesh.OriginMatrix, out Matrix4x4 invOriginMat);
                    invMat = Matrix4x4.Multiply(invMat, invOriginMat);

                    var rayStart = Vector3.Transform(progMan.MausLine3DPos, invMat);
                    var rayDir = Vector3.Transform(progMan.MausLine3DDir, invMat);
                    // Now trace the ray, in our simplification we assume the surface of the mesh we want to hit is coplanar to the xz plane
                    // Taken from: https://stackoverflow.com/a/18543221
                    Vector3 planeNrm = new(0, 1, 0);
                    float planeD = 0;
                    float dot = Vector3.Dot(planeNrm, rayDir);
                    var intersect = new Vector3();
                    if(Math.Abs(dot) > 1e-9)
                    {
                        var w = rayStart - planeNrm * planeD;
                        var fac = -Vector3.Dot(planeNrm, w) / dot;
                        var u = rayDir * fac;
                        intersect = rayStart + u;
                    }

                    Console.WriteLine($"  Clicked on {clickMesh.Filename_Int} at local coords: {(D3DVector)intersect}");
                }

                /*Console.WriteLine("".PadRight(Console.WindowWidth-1));
                try
                {
                    if (omsi.Globals.PlayerVehicle != null)
                    {
                        Console.WriteLine($"INEO_PS_Matricule: {omsi.Globals.PlayerVehicle.GetStringVariable("INEO_Login")}".PadRight(Console.WindowWidth - 1));


                        omsi.Globals.PlayerVehicle.SetStringVariable("INEO_Login", toggle ? "thomas" : "01234");
                        toggle = !toggle;
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Message); }*/

                Thread.Sleep(20);
            }
        }
    }

    public class DXTests
    {
        private readonly ManualResetEventSlim d3dGotContext = new(false);
        private OmsiHook.OmsiHook omsi;
        private const int texWidth = 256;
        private const int texHeight = 256;
        private D3DTexture.RGBA[] texBuffer = new D3DTexture.RGBA[texWidth * texHeight];
        private int iter = 0;
        private D3DTexture texture = null;

        public bool IsReady => texture != null && texture.IsValid;

        public void Init(OmsiHook.OmsiHook omsi)
        {
            texture = omsi.CreateTextureObject();
            this.omsi = omsi;
            omsi.OnOmsiGotD3DContext += Omsi_OnOmsiGotD3DContext;
            omsi.OnMapLoaded += Omsi_OnMapLoaded;
        }

        public void CreateTexture()
        {
            if (!omsi.IsD3DReady)
                return;

            try
            {
                texture.CreateD3DTexture(texWidth, texHeight).Wait();
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }

            var scriptTexes = omsi.Globals.PlayerVehicle.ComplObjInst.ScriptTextures;
            for (int i = 0; i < scriptTexes.Count; i++)
            {
                var old = scriptTexes[i];
                Console.WriteLine($"Replacing script tex: texPn:uint[{old.TexPn?.Length??-1}], color:{old.color}, tex:{old.tex} with tex:{texture.TextureAddress}");
                scriptTexes[i] = new()
                {
                    TexPn = old.TexPn,
                    color = old.color,
                    tex = unchecked((IntPtr)texture.TextureAddress)
                };
            }
        }

        public void UpdateTexture()
        {
            for (int y = 0; y < texHeight; y++)
                for(int x = 0; x < texWidth; x++)
                {
                    texBuffer[x + y * texWidth] = new D3DTexture.RGBA()
                    {
                        r = (byte)(((x + iter) * 4) % 256),
                        g = (byte)(((y + iter) * 4) % 256),
                        b = (byte)(((x + y + iter * 3) * 4) % 256),
                        a = (byte)(255 + iter * 127*0)
                    };
                }
            iter++;
            
            texture.UpdateTexture(texBuffer.AsMemory(), new OmsiRemoteMethods.Rectangle() { left=0, top=0, right=texWidth, bottom=texHeight}).Wait();
        }

        private void Omsi_OnOmsiGotD3DContext(object sender, EventArgs e)
        {
            // d3dGotContext.Set();
            Console.WriteLine("Got D3D Context!");
        }

        private void Omsi_OnMapLoaded(object sender, bool e)
        {
            d3dGotContext.Set();
            Console.WriteLine("Map Loaded!");
        }
    }
}
