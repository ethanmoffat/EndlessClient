// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Data.AccountCreation
{
	public class AccountCreateParameterException : Exception
	{
		public DATCONST1 Error { get; set; }

		public AccountCreateParameterException(DATCONST1 error)
		{
			Error = error;
		}
	}
}