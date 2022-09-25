using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DarkSide.Core
{
    public class PlayerCharacter
    {
        private EntityPosition _characterPosition;

        public EntityPosition CharacterPosition
        {
            get { return _characterPosition; }
            set { _characterPosition = value; }
        }

        public PlayerCharacter()
        {
            
        }
    }
}
