using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

namespace vrm
{
    [Flags]
    public enum Layers : int
    {
        Default = 0,
        Outlined = 8,
    }


    public class Methods
    {
        private Methods() { }

        // Helper method to find a child GameObject by name
        public static GameObject FindChildByName(GameObject parent, string name)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == name)
                    return child.gameObject;
            }
            return null;
        }

        // Helper method to find a child GameObject by tag
        public static GameObject FindChildWithTag(GameObject parent, string tag)
        {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.CompareTag(tag))
                    return child.gameObject;
            }

            return null;
        }
    }
}
