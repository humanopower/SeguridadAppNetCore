﻿using EntityLibrary;
using System;
using System.Collections.Generic;

namespace DataAccessContracts
{
	public interface IRoleOperationDA : IDisposable
	{
		int AddNewRoleOperation(RoleOperations roleOperations, User registerUser);

		int DelRoleOperation(RoleOperations roleOperations, User registerUser);

		List<Operation> GetoperationRole(Role role);

		List<RoleOperations> GetRoleOperations(Role role);
	}
}