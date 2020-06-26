using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Results;
using Railgun.Core.Enums;
using TreeDiagram;

namespace Railgun.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RoleLock : Attribute, IPreconditionAttribute
	{
		private readonly ModuleType _moduleType;

		public RoleLock(ModuleType modType) => _moduleType = modType;

		public Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services)
		{
			var user = context.Author as IGuildUser;
			var output = new StringBuilder();

			switch (_moduleType) 
			{
				case ModuleType.Music:
					var data = context.Database.ServerMusics.GetData(context.Guild.Id);

					if (data == null || data.AllowedRoles.Count < 1) return Task.FromResult(PreconditionResult.FromSuccess());

					var tempOutput = new StringBuilder();

					foreach (var allowedRole in data.AllowedRoles) {
						var role = context.Guild.GetRole(allowedRole.RoleId);

						tempOutput.AppendLine($"| {role.Name} |");

						if (user.RoleIds.Contains(allowedRole.RoleId)) return Task.FromResult(PreconditionResult.FromSuccess());
					}

					output.AppendLine("This command is locked to specific role(s). You must have the following role(s)...")
						.AppendLine()
						.AppendLine(tempOutput.ToString());
					break;
			}

			return Task.FromResult(PreconditionResult.FromError(output.ToString()));
		}
	}
}