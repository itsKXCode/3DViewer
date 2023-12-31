﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace _3DViewer.Utilities
{
    public static class ModelImporter
    {
        /// <summary>
        /// Imports a Compatible File (.stl, .obj, .ply)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static vtkPolyData ImportFile(string filePath)
        {
            switch (Path.GetExtension(filePath).ToLower())
            {
                case ".stl":
                    var stlReader = vtkSTLReader.New();
                    stlReader.SetFileName(filePath);
                    stlReader.Update();
                    return stlReader.GetOutput();
                case ".obj":
                    var objReader = vtkOBJReader.New();
                    objReader.SetFileName(filePath);
                    objReader.Update();
                    return objReader.GetOutput();
                case ".ply":
                    var plyReader = vtkPLYReader.New();
                    plyReader.SetFileName(filePath);
                    plyReader.Update();
                    return plyReader.GetOutput();
                case ".vtp":
                    var vtpReader = vtkXMLPolyDataReader.New();
                    vtpReader.SetFileName(filePath);
                    vtpReader.Update();
                    return vtpReader.GetOutput();
                default:
                    throw new NotImplementedException($"Filetype {Path.GetExtension(filePath)} not Supported");
            }

        }
    }
}
