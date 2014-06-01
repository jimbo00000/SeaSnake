// Version 1.8.3
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

namespace PrimitivesPro.Editor
{
    /// <summary>
    /// text definition of menu items
    /// </summary>
    static public class MenuDefinition
    {
        /// <summary>
        /// root menu path, by changing this you will change appearence in Unity Editor Menu
        /// </summary>
        private const string root = "GameObject/Create Other/PrimitivesPro/";

        public const string Arc =               root + "Arc";
        public const string Ellipse =           root + "Ellipse";
        public const string Circle =            root + "Circle";
        public const string Plane =             root + "Plane";
        public const string CuttingPlane =      root + "Cutting plane";
        public const string Ring =              root + "Ring";

        public const string Box =               root + "Box";
        public const string Capsule =           root + "Capsule";
        public const string Cone =              root + "Cone";
        public const string Cylinder =          root + "Cylinder";
        public const string Ellipsoid =         root + "Ellipsoid";
        public const string SuperEllipsoid =    root + "SuperEllipsoid";
        public const string GeoSphere =         root + "GeoSphere";
        public const string Pyramid =           root + "Pyramid";
        public const string RoundedCube =       root + "RoundedCube";
        public const string Sphere =            root + "Sphere";
        public const string SphericalCone =     root + "SphericalCone";
        public const string Torus =             root + "Torus";
        public const string TorusKnot =         root + "TorusKnot";
        public const string Tube =              root + "Tube";
        public const string Triangle =          root + "Triangle";
        public const string Boolean =           root + "Boolean operations";
    }
}
