using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ACaldeira.Combat;
using ACaldeira.Core;
using ACaldeira.Data;
using ACaldeira.Enemies;
using ACaldeira.Progression;
using ACaldeira.Simulation;
using ACaldeira.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ACaldeira.Editor
{
    public static class DieselpunkInstaller
    {
        private const string Base = "Assets/_Project/Generated";
        private const string ArtPath = "Assets/_Project/Art/Dieselpunk";
        private const string Bootstrap = Base + "/Scenes/00_Bootstrap.unity";
        private static Material material;
        private static Sprite steel;
        private static Sprite ground;
        private static TMP_FontAsset font;

        [MenuItem("Tools/A Caldeira/Aplicar artes dieselpunk")]
        public static void Install()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Pare o Play antes de aplicar as artes.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string[] files = { "actors", "weapons", "accessories", "props", "effects", "floor", "menu" };
            foreach (string file in files)
                if (!File.Exists(ArtPath + "/Atlases/" + file + ".png")) throw new FileNotFoundException(file);
            material = AssetDatabase.LoadAssetAtPath<Material>(Base + "/Art/Steel.mat");
            steel = AssetDatabase.LoadAssetAtPath<Sprite>(Base + "/Art/Steel.png");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Base + "/Art/InterfaceFont.asset");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            var art = AssetDatabase.LoadAssetAtPath<DieselpunkArt>(ArtPath + "/DieselpunkArt.asset");
            if (art == null) { art = ScriptableObject.CreateInstance<DieselpunkArt>(); AssetDatabase.CreateAsset(art, ArtPath + "/DieselpunkArt.asset"); }
            art.Actors = Slice("actors", new[] { "Engenheiro", "Drone", "Trator", "PrensaAutonoma" }, 2, 2,
                new[] { new Rect(0,.52f,.51f,.48f), new Rect(.56f,.55f,.44f,.45f), new Rect(0,0,.56f,.52f), new Rect(.56f,0,.44f,.55f) });
            art.Weapons = Slice("weapons", new[] { "Rebites", "Oleo", "Serras", "Estacas", "PrensaChoque", "CargaRetardo" }, 3, 2,
                new[] { new Rect(0,.51f,.33f,.49f), new Rect(.335f,.51f,.345f,.49f), new Rect(.695f,.51f,.305f,.49f), new Rect(0,0,.40f,.50f), new Rect(.405f,0,.26f,.50f), new Rect(.68f,0,.32f,.5f) });
            if(File.Exists(ArtPath+"/Atlases/saw-silver-v2.png"))art.Weapons[2]=Single("saw-silver-v2",false);
            art.Accessories = Slice("accessories", new[] { "Anel", "Fusivel", "Valvula", "Placa", "Bobina", "Sirene", "Cabo", "Sucata" }, 4, 2);
            art.Props = Slice("props", new[] { "Caldeira", "Barris", "Gerador", "Caixas", "Fornalha", "Barreira" }, 3, 2);
            art.Effects = Slice("effects", new[] { "Rebite", "Bolota", "Poca", "Estaca", "Alerta", "Impacto" }, 3, 2);
            art.Floor = Single("floor", true);
            ground=art.Floor;
            art.Menu = Single("menu", false);
            EditorUtility.SetDirty(art);
            foreach (string guid in AssetDatabase.FindAssets("t:WeaponSO", new[] { Base + "/Data" }))
            {
                var w = AssetDatabase.LoadAssetAtPath<WeaponSO>(AssetDatabase.GUIDToAssetPath(guid));
                Set(w, "icon", art.Weapon(w.Id));
            }
            string[] enemyNames = { "Drone de solda", "Trator sucatador", "Prensa autonoma" };
            for (int i = 0; i < enemyNames.Length; i++)
                Set(AssetDatabase.LoadAssetAtPath<EnemySO>(Base + "/Data/" + enemyNames[i] + ".asset"), "sprite", art.Actors[i + 1]);
            var yard = AssetDatabase.LoadAssetAtPath<StageSO>(Base + "/Data/Patio de Triagem.asset");
            Set(yard, "spawnArea", new Rect(-90,-60,180,120));
            Set(yard, "displayName", "Pátio de Triagem — Setor 07");
            Set(yard, "preview", art.Menu);
            var scene = EditorSceneManager.OpenScene(Bootstrap);
            var root = scene.GetRootGameObjects().First(g => g.GetComponent<GameManager>() != null);
            Set(root.GetComponent<WeaponManager>(), "maxWeaponSlots", 8);
            Set(root.GetComponent<RunProgression>(), "maxEquipmentSlots", 8);
            ApplyActors(root, art);
            ApplyUI(root, art);
            PlayerSettings.productName = "A Caldeira - Sobrevivencia de Ferro";
            PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.runInBackground = false;
            EditorSceneManager.SaveScene(scene);
            BuildYard(art);
            EditorSceneManager.OpenScene(Bootstrap);
            AssetDatabase.SaveAssets();
            File.WriteAllText("ARTES_INSTALADAS.txt", "Versão artística aplicada. Abra Assets/_Project/Generated/Scenes/00_Bootstrap.unity.\nMapa 180 x 120, sprites dieselpunk e interface integrados.\n");
            Debug.Log("DIESELPUNK_INSTALL_OK: 4 atores, 6 armas, 7 acessórios, sucata, 6 cenários, 6 efeitos; mapa 180x120.");
        }

        internal static Sprite[] Slice(string file, string[] names, int columns, int rows, Rect[] regions = null)
        {
            string path = ArtPath + "/Atlases/" + file + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true; importer.isReadable = true;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048; importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Color32[] pixels = texture.GetPixels32();
            var metas = new SpriteMetaData[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                Rect region = regions == null ? new Rect((i % columns) / (float)columns, 1f - (i / columns + 1f) / rows, 1f / columns, 1f / rows) : regions[i];
                int x0 = Mathf.RoundToInt(region.xMin * texture.width), y0 = Mathf.RoundToInt(region.yMin * texture.height);
                int x1 = Mathf.Min(texture.width, Mathf.RoundToInt(region.xMax * texture.width)), y1 = Mathf.Min(texture.height, Mathf.RoundToInt(region.yMax * texture.height));
                int left=x1, right=x0, bottom=y1, top=y0;
                if(file.EndsWith("-run"))
                {
                    RectInt component=PrimaryComponent(pixels,texture.width,x0,y0,x1,y1);
                    left=component.xMin;right=component.xMax-1;bottom=component.yMin;top=component.yMax-1;
                }
                else for (int y=y0; y<y1; y++) for (int x=x0; x<x1; x++)
                    if (pixels[y * texture.width+x].a > 96) { left=Mathf.Min(left,x); right=Mathf.Max(right,x); bottom=Mathf.Min(bottom,y); top=Mathf.Max(top,y); }
                if (right <= left || top <= bottom) throw new InvalidOperationException("Sprite vazio: " + names[i]);
                left=Mathf.Max(x0,left-3); bottom=Mathf.Max(y0,bottom-3); right=Mathf.Min(x1-1,right+3); top=Mathf.Min(y1-1,top+3);
                metas[i] = new SpriteMetaData { name=names[i], rect=new Rect(left,bottom,right-left+1,top-bottom+1), alignment=0, pivot=new Vector2(.5f,.5f) };
            }
            importer=(TextureImporter)AssetImporter.GetAtPath(path);
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var previous=provider.GetSpriteRects();
            var rects=metas.Select(m=>new SpriteRect {
                name=m.name,rect=m.rect,pivot=m.pivot,alignment=SpriteAlignment.Center,
                spriteID=previous.FirstOrDefault(r=>r.name==m.name)?.spriteID ?? GUID.Generate()
            }).ToArray();
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            return names.Select(n => sprites.First(s => s.name == n)).ToArray();
        }
        private static RectInt PrimaryComponent(Color32[] pixels,int textureWidth,int x0,int y0,int x1,int y1)
        {
            int width=x1-x0,height=y1-y0,bestCount=0;RectInt best=new RectInt();
            var visited=new bool[width*height];var queue=new Queue<int>();
            for(int localY=0;localY<height;localY++)for(int localX=0;localX<width;localX++)
            {
                int seed=localY*width+localX;
                if(visited[seed] || pixels[(y0+localY)*textureWidth+x0+localX].a<=96)continue;
                visited[seed]=true;queue.Enqueue(seed);int count=0,minX=localX,maxX=localX,minY=localY,maxY=localY;
                while(queue.Count>0)
                {
                    int current=queue.Dequeue(),cx=current%width,cy=current/width;count++;
                    minX=Mathf.Min(minX,cx);maxX=Mathf.Max(maxX,cx);minY=Mathf.Min(minY,cy);maxY=Mathf.Max(maxY,cy);
                    for(int oy=-1;oy<=1;oy++)for(int ox=-1;ox<=1;ox++)
                    {
                        if(ox==0&&oy==0)continue;int nx=cx+ox,ny=cy+oy;
                        if(nx<0||nx>=width||ny<0||ny>=height)continue;int next=ny*width+nx;
                        if(visited[next])continue;visited[next]=true;
                        if(pixels[(y0+ny)*textureWidth+x0+nx].a>96)queue.Enqueue(next);
                    }
                }
                if(count>bestCount){bestCount=count;best=new RectInt(x0+minX,y0+minY,maxX-minX+1,maxY-minY+1);}
            }
            if(bestCount==0)throw new InvalidOperationException("Nenhum componente visivel no quadro de animacao.");
            return best;
        }
        private static Sprite Single(string file, bool repeat)
        {
            string path=ArtPath + "/Atlases/"+file+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100; importer.mipmapEnabled=false;
            importer.wrapMode=repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=2048;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void ApplyActors(GameObject root, DieselpunkArt art)
        {
            var world=root.transform.Find("World");
            var player=world.Find("Engenheiro");
            player.localScale=Vector3.one;
            player.GetComponent<SpriteRenderer>().enabled=false;
            var visor=player.Find("Visor"); if (visor != null) visor.gameObject.SetActive(false);
            var body=player.Find("CorpoDieselpunk");
            if (body == null) body=WorldSprite("CorpoDieselpunk",player,art.Actors[0],Vector2.zero,1.7f,6).transform;
            Fit(body.GetComponent<SpriteRenderer>(),art.Actors[0],1.7f);
            var motion=player.GetComponent<DieselpunkMotion>(); if (motion == null) motion=player.gameObject.AddComponent<DieselpunkMotion>();
            motion.Body=body; motion.Visual=body.GetComponent<SpriteRenderer>();
            foreach (var actor in world.GetComponentsInChildren<EnemyActor>(true))
            {
                int index=actor.transform.parent.name.StartsWith("Drone") ? 1 : actor.transform.parent.name.StartsWith("Tractor") ? 2 : 3;
                Fit(actor.GetComponent<SpriteRenderer>(),art.Actors[index],index==1 ? 1.15f : index==2 ? 1.75f : 2.2f);
            }
            Sprite simpleProjectile=EnsureSimpleProjectileSprite();
            foreach (var actor in world.GetComponentsInChildren<ProjectileActor>(true))
            {
                string bank=actor.transform.parent.name;
                var renderer=actor.GetComponent<SpriteRenderer>();renderer.sharedMaterial=material;
                if(bank.StartsWith("Saw"))Fit(renderer,art.Weapons[2],.95f);
                else
                {
                    renderer.sprite=simpleProjectile;
                    if(bank.StartsWith("Flame")){renderer.color=new Color(.28f,.16f,.055f);actor.transform.localScale=new Vector3(.42f,.34f,1);}
                    else if(bank.StartsWith("Stake")){renderer.color=new Color(.68f,.84f,.9f);actor.transform.localScale=new Vector3(.82f,.16f,1);}
                    else {renderer.color=new Color(1f,.62f,.16f);actor.transform.localScale=new Vector3(.5f,.14f,1);}
                }
                Set(actor,"puddleSprite",art.Effects[2]);
            }
            foreach (var actor in world.GetComponentsInChildren<ACaldeira.Collectibles.CollectibleActor>(true))
                Fit(actor.GetComponent<SpriteRenderer>(),art.Accessories[7],.32f);
            foreach (var actor in world.GetComponentsInChildren<ACaldeira.World.HazardActor>(true))
                Fit(actor.GetComponent<SpriteRenderer>(),art.Effects[4],3f);
            var camera=root.GetComponentInChildren<Camera>(true);
            camera.backgroundColor=new Color(.055f,.065f,.067f);
            if(DirectionalArtInstaller.HasAtlases)DirectionalArtInstaller.ApplyToRoot(root);
        }
        private static Sprite EnsureSimpleProjectileSprite()
        {
            const string path=ArtPath+"/SimpleProjectile.asset";
            var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if(sprite!=null)return sprite;
            var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="SimpleProjectileTexture",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color32[32*32];
            Vector2 center=new Vector2(15.5f,15.5f);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {
                float distance=Vector2.Distance(new Vector2(x,y),center);
                byte alpha=(byte)Mathf.RoundToInt(Mathf.Clamp01(15.5f-distance)*255f);
                pixels[y*32+x]=new Color32(255,255,255,alpha);
            }
            texture.SetPixels32(pixels);texture.Apply(false,false);AssetDatabase.CreateAsset(texture,path);
            sprite=Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(.5f,.5f),32);sprite.name="SimpleProjectile";
            AssetDatabase.AddObjectToAsset(sprite,texture);EditorUtility.SetDirty(texture);AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
        }
        private static void Fit(SpriteRenderer renderer, Sprite sprite, float longest)
        {
            renderer.sprite=sprite; renderer.color=Color.white; renderer.sharedMaterial=material;
            float scale=longest/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y);
            renderer.transform.localScale=new Vector3(scale,scale,1);
        }
        private static GameObject WorldSprite(string name, Transform parent, Sprite sprite, Vector2 position, float size, int order)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=position;
            var renderer=go.AddComponent<SpriteRenderer>(); renderer.sortingOrder=order; Fit(renderer,sprite,size); return go;
        }
        private static void Plate(Transform parent,string name,Vector2 position,Vector2 size,Color tint,int order)
        {
            var go=WorldSprite(name,parent,steel,position,1,order);
            var renderer=go.GetComponent<SpriteRenderer>();
            if(size.x>1 && size.y>1)
            {
                renderer.sprite=ground;renderer.drawMode=SpriteDrawMode.Tiled;
                go.transform.localScale=Vector3.one;renderer.size=size;
            }
            else go.transform.localScale=new Vector3(size.x/steel.bounds.size.x,size.y/steel.bounds.size.y,1);
            renderer.color=tint;
        }
        private static void BuildYard(DieselpunkArt art)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("Pátio de Triagem / Distrito industrial");
            // 16-unit panels preserve texture detail across nine times the original area.
            for(int y=-88;y<88;y+=16) for(int x=-120;x<120;x+=16)
            {
                var tile=WorldSprite("Piso de aço",root.transform,art.Floor,new Vector2(x+8,y+8),16,-30);
                tile.GetComponent<SpriteRenderer>().color=new Color(.68f,.72f,.72f);
            }
            Color roadway=new Color(.47f,.52f,.53f), amber=new Color(.63f,.42f,.15f);
            foreach(int y in new[]{-38,0,38})
            {
                Plate(root.transform,"Via de transporte",new Vector2(0,y),new Vector2(184,9),roadway,-27);
                for(int x=-86;x<90;x+=8) Plate(root.transform,"Sinalização",new Vector2(x,y),new Vector2(3,.14f),amber,-25);
            }
            foreach(int x in new[]{-60,0,60})
            {
                Plate(root.transform,"Passagem",new Vector2(x,0),new Vector2(8,124),roadway,-27);
                for(int y=-54;y<60;y+=8) Plate(root.transform,"Sinalização",new Vector2(x,y),new Vector2(.14f,3),amber,-25);
            }
            // Perimeter machinery is outside the traversable rectangle.
            for(int x=-90;x<=90;x+=10)
            {
                WorldSprite("Barreira norte",root.transform,art.Props[5],new Vector2(x,63),9,-12);
                WorldSprite("Barreira sul",root.transform,art.Props[5],new Vector2(x,-63),9,-12);
            }
            for(int y=-53;y<=53;y+=10)
            {
                WorldSprite("Barreira oeste",root.transform,art.Props[5],new Vector2(-94,y),8,-12);
                WorldSprite("Barreira leste",root.transform,art.Props[5],new Vector2(94,y),8,-12);
            }
            var footprints=new List<Rect>();
            AddIsland(root.transform,art,footprints,-13,7,1,3.2f);
            AddIsland(root.transform,art,footprints,13,7,2,4.2f);
            AddIsland(root.transform,art,footprints,-13,-7,3,3.4f);
            AddIsland(root.transform,art,footprints,13,-7,0,4.2f);
            AddIsland(root.transform,art,footprints,-30,20,0,9);
            AddIsland(root.transform,art,footprints,30,20,4,9);
            AddIsland(root.transform,art,footprints,-30,-20,1,6);
            AddIsland(root.transform,art,footprints,30,-20,2,8);
            AddIsland(root.transform,art,footprints,-76,22,1,6);
            AddIsland(root.transform,art,footprints,-76,-22,3,6);
            AddIsland(root.transform,art,footprints,76,22,2,8);
            AddIsland(root.transform,art,footprints,76,-22,4,8);
            AddIsland(root.transform,art,footprints,-30,51,0,9);
            AddIsland(root.transform,art,footprints,30,51,0,9);
            AddIsland(root.transform,art,footprints,-30,-51,3,7);
            AddIsland(root.transform,art,footprints,30,-51,3,7);
            root.AddComponent<ACaldeira.World.YardObstacles>().Footprints=footprints.ToArray();
            GroundLabel(root.transform,"07",new Vector2(0,6),2.4f,amber);
            GroundLabel(root.transform,"TRIAGEM",new Vector2(0,-9),1.25f,amber);
            GroundLabel(root.transform,"COMBUSTÍVEL",new Vector2(-60,48),1.1f,amber);
            GroundLabel(root.transform,"CASA DAS MÁQUINAS",new Vector2(60,48),1.1f,amber);
            EditorSceneManager.SaveScene(scene,Base+"/Scenes/10_PatioTriagem.unity");
        }
        private static void AddIsland(Transform root,DieselpunkArt art,List<Rect> footprints,float x,float y,int prop,float size)
        {
            Plate(root,"Base da máquina",new Vector2(x,y-1),new Vector2(size+3,size*.72f),new Color(.42f,.46f,.45f),-22);
            WorldSprite("Instalação / "+art.Props[prop].name,root,art.Props[prop],new Vector2(x,y),size,-10);
            footprints.Add(new Rect(x-size*.42f,y-size*.28f,size*.84f,size*.48f));
            WorldSprite("Sucata de cenário",root,art.Props[3],new Vector2(x+size*.64f,y-2),2.5f,-11);
        }
        private static void GroundLabel(Transform root,string text,Vector2 position,float size,Color color)
        {
            var go=new GameObject(text); go.transform.SetParent(root,false); go.transform.position=position;
            var label=go.AddComponent<TextMeshPro>(); label.font=font; label.text=text; label.fontSize=size*10;
            label.color=new Color(color.r,color.g,color.b,.65f); label.alignment=TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta=new Vector2(30,10); label.GetComponent<MeshRenderer>().sortingOrder=-24;
        }
        private static void ApplyUI(GameObject root,DieselpunkArt art)
        {
            var canvas=root.GetComponentInChildren<Canvas>(true);
            var safe=canvas.transform.Find("SafeArea");
            var principal=safe.Find("Menu/Principal");
            var menu=principal.parent;
            var image=principal.GetComponent<Image>(); if(image!=null) image.color=Color.clear;
            var bg=principal.Find("ArteFundo");
            if(bg==null)
            {
                var graphic=UIImage(principal,"ArteFundo",Vector2.zero,new Vector2(1280,720),art.Menu,Color.white);
                var rt=graphic.rectTransform; rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.sizeDelta=Vector2.zero;
                graphic.transform.SetAsFirstSibling();
            }
            var labels=principal.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.transform.parent==principal).ToArray();
            for(int i=0;i<labels.Length;i++)
            {
                labels[i].rectTransform.anchoredPosition=new Vector2(-345,labels[i].rectTransform.anchoredPosition.y);
                labels[i].rectTransform.sizeDelta=new Vector2(510,100);
            }
            if(labels.Length>=4)
            {
                labels[0].text="EDIÇÃO DE TESTE  /  DIESELPUNK"; labels[0].fontSize=16;
                labels[1].fontSize=58;labels[2].text="SOBREVIVÊNCIA DE FERRO";
                labels[3].text="WASD / SETAS   •   DISPARO AUTOMÁTICO\nUM PÁTIO. UM TURNO. SOBREVIVA.";labels[3].fontSize=15;
            }
            int bi=0;
            foreach(var button in principal.GetComponentsInChildren<Button>(true))
            {
                if(button.name.Contains("MONTAGEM")||button.name.Contains("CARGA")) { button.gameObject.SetActive(false);continue; }
                var rt=(RectTransform)button.transform;rt.anchoredPosition=new Vector2(-345,50-bi*66);rt.sizeDelta=new Vector2(410,52);bi++;
            }
            var choices=safe.Find("Fundicao");
            var choiceButtons=choices.GetComponentsInChildren<Button>(true).Where(b=>b.name=="Upgrade").ToArray();
            var ui=canvas.GetComponent<GameplayUI>();
            var theme=canvas.GetComponent<DieselpunkInterface>(); if(theme==null)theme=canvas.gameObject.AddComponent<DieselpunkInterface>();
            theme.Art=art;theme.Progression=root.GetComponent<RunProgression>();theme.Weapons=root.GetComponent<WeaponManager>();
            theme.Simulation=root.GetComponent<GameplaySimulation>();theme.Game=root.GetComponent<GameManager>();theme.Player=root.transform.Find("World/Engenheiro");
            theme.ChoiceIcons=new Image[3];theme.ChoiceKinds=new TMP_Text[3];theme.ChoiceFrames=new Image[3];
            foreach(var label in choices.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.transform.parent==choices && t.name!="Instrucao"))
            {label.text="APRIMORAR EXOTRAJE";label.rectTransform.anchoredPosition=new Vector2(0,272);}
            for(int i=0;i<3;i++)
            {
                var button=choiceButtons[i];var rt=(RectTransform)button.transform;
                rt.anchoredPosition=new Vector2((i-1)*360,12);rt.sizeDelta=new Vector2(336,390);
                button.GetComponent<Image>().color=new Color(.09f,.115f,.12f,.99f);
                var label=button.GetComponentInChildren<TMP_Text>();label.fontSize=21;
                label.rectTransform.anchoredPosition=new Vector2(0,-100);label.rectTransform.sizeDelta=new Vector2(305,158);
                theme.ChoiceFrames[i]=GetOrImage(rt,"Friso",new Vector2(0,190),new Vector2(336,4),null,Color.white);
                theme.ChoiceIcons[i]=GetOrImage(rt,"Icone",new Vector2(0,52),new Vector2(172,160),art.Weapons[i],Color.white);
                theme.ChoiceKinds[i]=GetOrLabel(rt,"Categoria","EQUIPAMENTO",new Vector2(0,163),new Vector2(315,28),12);
            }
            var reroll=choices.Find("RerrolarArtes");
            if(reroll==null)
            {
                var img=UIImage(choices,"RerrolarArtes",new Vector2(0,-251),new Vector2(290,48),null,new Color(.17f,.24f,.23f));
                img.raycastTarget=true;
                theme.Reroll=img.gameObject.AddComponent<Button>();theme.Reroll.targetGraphic=img;
            }
            else theme.Reroll=reroll.GetComponent<Button>();
            theme.RerollLabel=GetOrLabel(theme.Reroll.transform,"Legenda","RERROLAR [R] / 3",Vector2.zero,new Vector2(280,46),19);
            GetOrLabel(choices,"Instrucao","ESCOLHA UM CARD  /  CLIQUE OU ENTER / ESPAÇO",new Vector2(0,-307),new Vector2(850,30),14);
            var hud=safe.Find("HUD");
            theme.EquipmentIcons=new Image[8];theme.EquipmentLabels=new TMP_Text[8];
            for(int i=0;i<8;i++)
            {
                var slot=GetOrImage(hud,"SlotArte"+i,new Vector2((i-3.5f)*62,261),new Vector2(56,64),null,new Color(.055f,.075f,.075f,.92f));
                theme.EquipmentIcons[i]=GetOrImage(slot.transform,"Icone",new Vector2(0,6),new Vector2(42,42),art.Weapons[0],Color.white);
                theme.EquipmentLabels[i]=GetOrLabel(slot.transform,"Nivel","—",new Vector2(0,-23),new Vector2(54,16),11);
            }
            theme.SectorLabel=GetOrLabel(hud,"SetorArte","SETOR 07 / TRIAGEM",new Vector2(0,212),new Vector2(650,26),14);
            theme.SectorLabel.color=new Color(.81f,.73f,.54f);
            var result=safe.Find("Resultado").GetComponentsInChildren<TMP_Text>(true).First(t=>t.transform.parent.name=="Resultado");
            result.fontSize=28;result.rectTransform.sizeDelta=new Vector2(1000,260);result.rectTransform.anchoredPosition=new Vector2(0,170);
        }
        private static Image GetOrImage(Transform parent,string name,Vector2 pos,Vector2 size,Sprite sprite,Color color)
        { var existing=parent.Find(name);return existing!=null?existing.GetComponent<Image>():UIImage(parent,name,pos,size,sprite,color); }
        private static Image UIImage(Transform parent,string name,Vector2 pos,Vector2 size,Sprite sprite,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform));var rt=(RectTransform)go.transform;rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;
            var image=go.AddComponent<Image>();image.sprite=sprite;image.color=color;image.preserveAspect=sprite!=null;image.raycastTarget=false;return image;
        }
        private static TMP_Text GetOrLabel(Transform parent,string name,string text,Vector2 pos,Vector2 size,float fontSize)
        {
            var existing=parent.Find(name);
            if(existing!=null)
            {
                var current=existing.GetComponent<TMP_Text>();current.text=text;current.fontSize=fontSize;
                current.rectTransform.anchoredPosition=pos;current.rectTransform.sizeDelta=size;
                return current;
            }
            var go=new GameObject(name,typeof(RectTransform));var rt=(RectTransform)go.transform;rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;
            var label=go.AddComponent<TextMeshProUGUI>();label.font=font;label.fontSize=fontSize;label.text=text;
            label.color=new Color(.86f,.86f,.8f);label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;return label;
        }
        internal static void Set(Object target,string property,object value)
        {
            if(target==null)throw new InvalidOperationException("Missing target: "+property);
            var so=new SerializedObject(target);var p=so.FindProperty(property);
            if(p==null)throw new InvalidOperationException(target.name+" missing "+property);
            if(value is Object o)p.objectReferenceValue=o;
            else if(value is Rect rect)p.rectValue=rect;
            else if(value is string s)p.stringValue=s;
            else if(value is int integer)p.intValue=integer;
            else if(value is float number)p.floatValue=number;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);
        }
    }
}
