using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// http://wiki.unity3d.com/index.php?title=ObjExporter
/// </summary>
public class ObjExporter
{
    public static string MeshToString(MeshFilter mf)
    {
        var m = mf.sharedMesh;

        var sb = new StringBuilder();

        sb.AppendLine("# Exported from PrimitivesPro, Unity3D asset");
        sb.AppendLine("# http://u3d.as/4gQ");
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine();

        if (mf.renderer && mf.renderer.sharedMaterial)
        {
            sb.Append("mtllib ").Append(mf.renderer.sharedMaterial.name + ".mtl").Append("\n");
        }

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (var v in m.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (var v in m.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (var v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        for (var material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append("default").Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            }
        }
        return sb.ToString();
    }

    public static void MeshToFile(MeshFilter mf, string rootPath, string filename, bool exportMaterial)
    {
        var objFile = rootPath + "/" + filename + ".obj";

        using (var sw = new StreamWriter(objFile))
        {
            sw.Write(MeshToString(mf));
        }

        if (mf.renderer && mf.renderer.sharedMaterial)
        {
            var materialFile = rootPath + "/" + mf.renderer.sharedMaterial.name + ".mtl";

            using (var sw = new StreamWriter(materialFile))
            {
                sw.Write(MaterialToFile(mf.renderer.sharedMaterial, materialFile, rootPath));
            }
        }
    }

    public static string MaterialToFile(Material material, string filename, string root)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Exported from PrimitivesPro, Unity3D asset");
        sb.AppendLine("# http://u3d.as/4gQ");
        sb.AppendLine("--------------------------------------------");
        sb.AppendLine();

        sb.AppendLine("newmtl default");
        sb.AppendLine("Ka  0.6 0.6 0.6");
        sb.AppendLine("Kd  0.6 0.6 0.6");
        sb.AppendLine("Ks  0.9 0.9 0.9");
        sb.AppendLine("d  1.0");
        sb.AppendLine("Ns  0.0");
        sb.AppendLine("illum 2");

        if (material.name.Length > 0)
        {
            var file = FindTextureFile(material.name);

            if (file != null)
            {
                try
                {
                    File.Copy(file, root + "/" + Path.GetFileName(file), true);
                    sb.AppendLine("map_Kd " + Path.GetFileName(file));
                }
                catch(Exception ex)
                {
                    Debug.Log("Error copy texture file! " + ex.Message);
                }
            }
        }

        return sb.ToString();
    }

    private static string FindTextureFile(string name)
    {
#if !UNITY_WEBPLAYER
        // cut last number symbol to prevent unity some crazy renaming in case of duplicate materials
        var searchName = name;
        if (char.IsNumber(searchName[name.Length - 1]))
        {
            searchName = searchName.Remove(name.Length - 1, 1);

            if (searchName.Length > 0 && searchName[searchName.Length - 1] == ' ')
            {
                searchName = searchName.Remove(searchName.Length - 1, 1);
            }
        }

        var hdDirectoryInWhichToSearch = new DirectoryInfo(Application.dataPath);
        var filesInDir = hdDirectoryInWhichToSearch.GetFiles("*"+ searchName + "*.*", SearchOption.AllDirectories);

        foreach (var foundFile in filesInDir)
        {
            switch (foundFile.Extension.ToLower())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".tga":
                    return foundFile.FullName;
            }
        }

        return null;
#else
        return null;
#endif
    }
}
