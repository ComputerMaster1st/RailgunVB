using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Fun;
using TreeDiagram.Models.Server;

namespace TreeDiagram.Models
{
    public class ServerProfile : IdBase
    {
        public virtual ServerFilters Filters { get; private set; } = new ServerFilters();
        public virtual ServerFun Fun { get; private set; } = new ServerFun();
        public virtual ServerCommand Command { get; private set; } = new ServerCommand();
        public virtual ServerGlobals Globals { get; private set; } = new ServerGlobals();
        public virtual ServerInactivity Inactivity { get; private set; } = new ServerInactivity();
        public virtual ServerJoinLeave JoinLeave { get; private set; } = new ServerJoinLeave();
        public virtual ServerMusic Music { get; private set; } = new ServerMusic();
        public virtual ServerRoleRequest RoleRequest { get; private set; } = new ServerRoleRequest();
        public virtual ServerWarning Warning { get; private set; } = new ServerWarning();
        
        public ServerProfile(ulong id) : base(id) {}
    }
}