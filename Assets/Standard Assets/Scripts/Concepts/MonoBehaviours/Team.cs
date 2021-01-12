using UnityEngine;

namespace ArcherGame
{
    public class Team : MonoBehaviour
    {
		public Color color;
		public Player Representative
        {
            get
            {
                return representatives[0];
            }
            set
            {
                representatives[0] = value;
            }
        }
		public Player[] representatives;
        public Team Opponent
        {
            get
            {
                return opponents[0];
            }
            set
            {
                opponents[0] = value;
            }
        }
        public Team[] opponents;
    }
}