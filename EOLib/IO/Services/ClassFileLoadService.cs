// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Services
{
	public class ClassFileLoadService : IPubLoadService<ClassRecord>
	{
		public IDataFile<ClassRecord> LoadPubFromDefaultFile()
		{
			return LoadPubFromExplicitFile(Constants.ClassFilePath);
		}

		public IDataFile<ClassRecord> LoadPubFromExplicitFile(string fileName)
		{
			var classFile = new ClassFile();
			classFile.Load(fileName);
			return classFile;
		}
	}
}
