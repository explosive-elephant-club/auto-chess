//---------------------------
//主要功能：提取预制体缩略图
//---------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class LoadPrefabTexture : MonoBehaviour
{
    string GetTotalPath(string folder)
    {
        // 设置输出路径
        string path;
        path = Application.dataPath + "/../Assets/Resources/" + _prefabPath;
        path += folder;
        path += "/";
        return path;
    }

    string GetAssetPath(string folder)
    {
        // 设置输出路径
        string path;
        path = "Assets/Resources/" + _prefabPath;
        path += folder;
        path += "/";
        return path;
    }

    void Start()
    {
        /*for (int i = 1; i < 100; i++)
        {
            Directory.CreateDirectory(Application.dataPath + "/../Assets/Resources/Prefab/Projectile/Skill/" + i + "/");
        }
        */

        
        if (!Directory.Exists("Assets/Resources/" + _prefabPath))//判断文件夹是否存在
        {
            Debug.Log("不存在");
            return;
        }
        Debug.Log("存在");

        Directory.CreateDirectory(GetTotalPath(_targetFolder));
        Directory.CreateDirectory(GetTotalPath(_bgFolder));
       

        // 提取缩略图
        LoadTexture();

        // 清空背景色(等文件生成完)
        //Invoke("ClearBackground", _count * 0.2f);
    }

    // 提取缩略图
    public void LoadTexture()
    {
        DirectoryInfo directory = new DirectoryInfo("Assets/Resources/" + _prefabPath);
        FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);//查找改路径下的所有文件夹，包含子文件夹

        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith(".prefab"))
            {
                continue;
            }
            string strBaseName = files[i].Name.Replace(".prefab", "");

            GameObject prefab = Resources.Load<GameObject>(_prefabPath + strBaseName);
            Debug.Log(prefab);
            // 获取缩略图
            Texture2D Tex = AssetPreview.GetAssetPreview(prefab);
            Debug.Log(Tex);
            if (Tex != null)
            {

                byte[] bytes = Tex.EncodeToPNG();
                string totalPath = GetTotalPath(_bgFolder) + strBaseName + ".png";
                File.WriteAllBytes(totalPath, bytes);
                ++_count;
            }
        }
    }

    // 清空背景色
    public void ClearBackground()
    {
        DirectoryInfo directory = new DirectoryInfo("Assets/Resources/" + _prefabPath + _bgFolder + "/");
        FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);//查找改路径下的所有文件夹，包含子文件夹

        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Name.EndsWith(".png"))
            {
                continue;
            }
            Debug.Log("files[i].Name:" + files[i].Name);

            string strBaseName = files[i].Name.Replace(".png", "");
            string totalPath = GetTotalPath(_bgFolder) + strBaseName + ".png";
            if (File.Exists(totalPath))
            {
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(GetAssetPath(_bgFolder) + strBaseName + ".png"); // 获取文件
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                //importer.textureType = TextureImporterType.Sprite; // 修改属性
                importer.SaveAndReimport(); // 一定要记得写上这句
            }

            m_CurrentTexturePath = GetAssetPath(_bgFolder) + files[i].Name;
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(m_CurrentTexturePath);
            texture2D = DeCompress(texture2D);

            for (int m = 0; m < texture2D.width; m++)
            {
                for (int n = 0; n < texture2D.height; n++)
                {
                    Color color = texture2D.GetPixel(m, n);
                    if (color == _srcColor)
                    {
                        texture2D.SetPixel(m, n, _dstColor);
                    }

                }
            }

            // 设置透明
            if (!texture2D.alphaIsTransparency)
            {
                texture2D.alphaIsTransparency = true;
            }

            //实际应用前面的SetPixel和Setpixels的更改，注意应用的时机，要在处理完一张图片之后再进行应用
            texture2D.Apply();
            byte[] bytes = texture2D.EncodeToPNG();


            using (FileStream fileStream = new FileStream(GetTotalPath(_targetFolder) + files[i].Name, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }
    }

    // 解压缩图片
    public Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    [Header("手动取一下缩略图的背景色")]
    public Color _srcColor; // 要改的原始颜色

    [Header("改后的背景色（要透明就把alpha调成0）")]
    public Color _dstColor; // 改后的目标颜色

    // prefab预制体必须放在Resources目录下
    [Header("Resources文件夹下的相对路径")]
    public string _prefabPath;

    [Header("带背景的缩略图文件夹")]
    public string _bgFolder = "Textures With Background"; // 输出文件夹
    [Header("缩略图文件夹")]
    public string _targetFolder = "Textures"; // 输出文件夹

    private string m_CurrentTexturePath = null;//具体图片
    private int _count = 0;
}