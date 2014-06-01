// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace PrimitivesPro.Primitives
{
    /// <summary>
    /// type of normal
    /// </summary>
    public enum NormalsType
    {
        /// <summary>
        /// normal vector is computing by averaging adjacent face normals (smooth shading)
        /// </summary>
        Vertex,

        /// <summary>
        /// normal vector is equal normal vector of the triangle face (flat shading)
        /// 
        /// IMPORTANT NOTE:
        /// Because Unity supports only per-vertex normals, generating Face normals
        /// will result in higher number of vertices up to 4x more then in Vertex case!
        /// To minimize vertex count consider shader-based solution for flat-shading
        /// effect.
        /// </summary>
        Face,
    }

    /// <summary>
    /// position of the model pivot
    /// </summary>
    public enum PivotPosition
    {
        /// <summary>
        /// pivot is at bottom
        /// </summary>
        Botttom,

        /// <summary>
        /// pivot is in center
        /// </summary>
        Center,

        /// <summary>
        /// pivot is on top
        /// </summary>
        Top,
    }

    public abstract class Primitive
    {
    }
}
