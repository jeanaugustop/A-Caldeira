using System;
using System.IO;
using System.Linq;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Enemies;
using ACaldeira.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ACaldeira.Editor
{
    public static class DirectionalArtInstaller
    {
        private const string Art="Assets/_Project/Art/Dieselpunk";
        private const string Generated="Assets/_Project/Generated";
        private static readonly string[] Names={"engineer-directions","drone-directions","tractor-directions","press-directions"};
        private static readonly string[] RunNames={"engineer-run","drone-run","tractor-run","press-run"};
        private static readonly string[] Compass={"S","SW","W","NW","N","NE","E","SE"};
        private static readonly float[] Sizes={1.8f,1.0f,4.2f,2.8f};
        private static readonly float[] Radius={.35f,.3f,1.05f,.75f};
        public static bool HasAtlases=>Names.All(n=>File.Exists(Art+"/Atlases/"+n+".png"));

        [MenuItem("Tools/A Caldeira/Aplicar sprites direcionais")]
        public static void Install()
        {
            if(Application.isPlaying)
                throw new InvalidOperationException("Feche o Play antes de aplicar os sprites direcionais.");
            if(!HasAtlases)throw new FileNotFoundException("Faltam pranchas direcionais.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var scene=EditorSceneManager.OpenScene(Generated+"/Scenes/00_Bootstrap.unity");
            var root=scene.GetRootGameObjects().First(g=>g.GetComponent<GameManager>()!=null);
            ApplyToRoot(root);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("DIRECTIONAL_ART_OK: 32 poses paradas e 128 quadros de movimento; escala drone 1 / engenheiro 1.8 / prensa 2.8 / trator 4.2.");
        }

        internal static void ApplyToRoot(GameObject root)
        {
            var sets=new DirectionalSprites[4];
            for(int i=0;i<4;i++)
            {
                string path=Art+"/"+Names[i]+".asset";
                var set=AssetDatabase.LoadAssetAtPath<DirectionalSprites>(path);
                if(set==null){set=ScriptableObject.CreateInstance<DirectionalSprites>();AssetDatabase.CreateAsset(set,path);}
                Rect[] regions=null;
                if(i==2)
                {
                    regions=new Rect[8];
                    float[] edges={0,.225f,.493f,.75f,1};
                    for(int frame=0;frame<8;frame++)regions[frame]=new Rect(edges[frame%4],frame<4?.5f:0,edges[frame%4+1]-edges[frame%4],.5f);
                }
                set.Frames=DieselpunkInstaller.Slice(Names[i],Compass.Select(c=>Names[i]+"-"+c).ToArray(),4,2,regions);
                // Actual sheets are visually reviewed; two profiles came in opposite cell order.
                if(i==1 || i==3){var swap=set.Frames[2];set.Frames[2]=set.Frames[6];set.Frames[6]=swap;}
                set.Mirror=new bool[8];
                if(i==0 || i==1)set.Mirror[1]=true;
                string runPath=Art+"/Atlases/"+RunNames[i]+".png";
                if(File.Exists(runPath))
                {
                    string[] frameNames=Compass.SelectMany(c=>Enumerable.Range(1,4).Select(f=>RunNames[i]+"-"+c+"-"+f)).ToArray();
                    set.MoveFrames=DieselpunkInstaller.Slice(RunNames[i],frameNames,4,8);
                    var runTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(runPath);
                    if(runTexture.GetPixel(0,0).a>.1f)throw new InvalidOperationException(RunNames[i]+" nao possui transparencia importada.");
                }
                if(i==0 && File.Exists(Art+"/Atlases/engineer-sides-v2-run.png"))
                {
                    string sideName="engineer-sides-v2-run";
                    var sideFrames=DieselpunkInstaller.Slice(sideName,
                        new[]{"Engineer-W-1","Engineer-W-2","Engineer-W-3","Engineer-W-4","Engineer-E-1","Engineer-E-2","Engineer-E-3","Engineer-E-4"},4,2);
                    for(int frame=0;frame<4;frame++)
                    {
                        set.MoveFrames[2*4+frame]=sideFrames[frame];
                        set.MoveFrames[6*4+frame]=sideFrames[4+frame];
                    }
                }
                set.AnimationFramesPerUnit=i==0?3.2f:i==1?4.5f:i==2?2.0f:2.5f;
                set.WorldSize=Sizes[i];set.BobAmplitude=i==2?0:i==3?.015f:.035f;set.BobFrequency=i==3?8:12;
                EditorUtility.SetDirty(set);sets[i]=set;
            }
            var world=root.transform.Find("World");
            var player=world.Find("Engenheiro");
            Wire(player,player.Find("CorpoDieselpunk").GetComponent<SpriteRenderer>(),sets[0]);
            foreach(var actor in world.GetComponentsInChildren<EnemyActor>(true))
            {
                int index=actor.transform.parent.name.StartsWith("Drone")?1:actor.transform.parent.name.StartsWith("Tractor")?2:3;
                var old=actor.GetComponent<SpriteRenderer>();old.enabled=false;
                actor.transform.localScale=Vector3.one;
                var child=actor.transform.Find("CorpoDirecional");
                if(child==null)
                {
                    child=new GameObject("CorpoDirecional",typeof(SpriteRenderer)).transform;
                    child.SetParent(actor.transform,false);
                }
                var visual=child.GetComponent<SpriteRenderer>();visual.sharedMaterial=old.sharedMaterial;
                visual.sortingOrder=old.sortingOrder;visual.color=Color.white;
                Wire(actor.transform,visual,sets[index]);
            }
            string[] enemies={"Drone de solda","Trator sucatador","Prensa autonoma"};
            for(int i=0;i<3;i++)
            {
                var enemy=AssetDatabase.LoadAssetAtPath<EnemySO>(Generated+"/Data/"+enemies[i]+".asset");
                DieselpunkInstaller.Set(enemy,"sprite",sets[i+1].Frames[0]);
                DieselpunkInstaller.Set(enemy,"collisionRadius",Radius[i+1]);
            }
        }
        private static void Wire(Transform actor,SpriteRenderer visual,DirectionalSprites sprites)
        {
            var directional=actor.GetComponent<DirectionalActor>();
            if(directional==null)directional=actor.gameObject.AddComponent<DirectionalActor>();
            directional.Visual=visual;directional.Sprites=sprites;directional.ResetFacing();
            EditorUtility.SetDirty(directional);
        }
    }
}
