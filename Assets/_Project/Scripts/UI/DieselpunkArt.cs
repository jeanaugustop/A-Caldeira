using UnityEngine;

namespace ACaldeira.UI
{
    public sealed class DieselpunkArt : ScriptableObject
    {
        public Sprite[] Actors;
        public Sprite[] Weapons;
        public Sprite[] Accessories;
        public Sprite[] Props;
        public Sprite[] Effects;
        public Sprite Floor;
        public Sprite Menu;

        public Sprite Weapon(string id)
        {
            int index = id == "Oleo Cru" ? 1 : id == "Serras Orbitais" ? 2 :
                id == "Estacas Hidraulicas" ? 3 : id == "Prensa de Choque" ? 4 :
                id == "Carga de Retardo" ? 5 : 0;
            return Weapons[index];
        }
    }
}
