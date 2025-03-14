using PlayerRoles;

namespace SCP008X
{
    public static class Extensions
    {
        public static bool NonHuman(this RoleTypeId role, bool onlyScPs)
        {
            if (onlyScPs)
            {
                switch (role)
                {
                    case RoleTypeId.Scp049:
                    case RoleTypeId.Scp0492:
                    case RoleTypeId.Scp079:
                    case RoleTypeId.Scp096:
                    case RoleTypeId.Scp106:
                    case RoleTypeId.Scp173:
                    case RoleTypeId.Scp939:
                        return true;
                    default:
                        return false;
                }
            }

            switch (role)
            {
                case RoleTypeId.Scp049:
                case RoleTypeId.Scp0492:
                case RoleTypeId.Scp079:
                case RoleTypeId.Scp096:
                case RoleTypeId.Scp106:
                case RoleTypeId.Scp173:
                case RoleTypeId.Scp939:
                case RoleTypeId.Spectator:
                case RoleTypeId.None:
                    return true;
                default:
                    return false;
            }
        }
        public static bool Gun(this ItemType Item)
        {
            switch (Item)
            {
                case ItemType.GunCOM15:
                case ItemType.GunE11SR:
                case ItemType.GunLogicer:
                case ItemType.MicroHID:
                    return true;
                default:
                    return false;
            }
        }
    }
}
