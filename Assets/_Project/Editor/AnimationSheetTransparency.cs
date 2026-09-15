using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ACaldeira.Editor
{
    // The generated movement sheets contain a neutral preview checker. The source PNG stays
    // untouched; only the Unity-imported texture receives alpha through an edge flood fill.
    public sealed class AnimationSheetTransparency : AssetPostprocessor
    {
        private bool IsMovementSheet => assetPath.Contains("/_Project/Art/Dieselpunk/Atlases/") && assetPath.EndsWith("-run.png");

        private void OnPreprocessTexture()
        {
            if(!IsMovementSheet)return;
            var importer=(TextureImporter)assetImporter;
            importer.isReadable=true;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.alphaSource=TextureImporterAlphaSource.FromGrayScale;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            if(!IsMovementSheet)return;
            Color32[] pixels=texture.GetPixels32();int width=texture.width,height=texture.height;
            for(int i=0;i<pixels.Length;i++){var opaque=pixels[i];opaque.a=255;pixels[i]=opaque;}
            var seen=new bool[pixels.Length];var queue=new Queue<int>();
            for(int x=0;x<width;x++){TryAdd(x,pixels,seen,queue);TryAdd((height-1)*width+x,pixels,seen,queue);}
            for(int y=0;y<height;y++){TryAdd(y*width,pixels,seen,queue);TryAdd(y*width+width-1,pixels,seen,queue);}
            while(queue.Count>0)
            {
                int i=queue.Dequeue(),x=i%width,y=i/width;
                var c=pixels[i];c.a=0;pixels[i]=c;
                if(x>0)TryAdd(i-1,pixels,seen,queue);if(x<width-1)TryAdd(i+1,pixels,seen,queue);
                if(y>0)TryAdd(i-width,pixels,seen,queue);if(y<height-1)TryAdd(i+width,pixels,seen,queue);
            }
            texture.SetPixels32(pixels);texture.Apply(false,false);
        }

        private static void TryAdd(int index,Color32[] pixels,bool[] seen,Queue<int> queue)
        {
            if(seen[index])return;seen[index]=true;Color32 c=pixels[index];
            int maximum=Mathf.Max(c.r,Mathf.Max(c.g,c.b));int minimum=Mathf.Min(c.r,Mathf.Min(c.g,c.b));
            if(maximum-minimum<=24 && minimum>=135)queue.Enqueue(index);
        }
    }
}
