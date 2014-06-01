// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

//
// This CSG library is based on javascript library made by Evan Wallace
// http://evanw.github.io/csg.js/ distributed under MIT license.
// Copyright (c) 2011 Evan Wallace (http://madebyevan.com/), under the MIT license.
//
// For more information about algorithm, see http://www.mcs.csueastbay.edu/~tebo/papers/siggraph90.pdf.
//

using UnityEngine;

namespace PrimitivesPro.CSG
{
    /// <summary>
    /// represents vertex with position, normal and uv
    /// </summary>
    public class CSGVertex
    {
        /// <summary>
        /// constructor
        /// </summary>
        public CSGVertex(Vector3 p, Vector3 n, Vector2 uv)
        {
            P = p;
            N = n;
            UV = uv;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public CSGVertex(CSGVertex v)
        {
            P = v.P;
            N = v.N;
            UV = v.UV;
        }

        /// <summary>
        /// position
        /// </summary>
        public Vector3 P;

        /// <summary>
        /// normal
        /// </summary>
        public Vector3 N;

        /// <summary>
        /// uv
        /// </summary>
        public Vector2 UV;

        /// <summary>
        /// flip normal vector
        /// </summary>
        public void Flip()
        {
            N = -N;
        }
    }
}
