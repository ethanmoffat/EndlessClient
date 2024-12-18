﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace EOLib.Config.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class IniReaderTest
    {
        private const string TestDirectory = "IniReaderTest";
        private const string TestFile = "SomeFile.ini";
        private static readonly string FullPath = Path.Combine(TestDirectory, TestFile);

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestDirectory))
                Directory.Delete(TestDirectory, true);
        }

        [Test]
        public void Load_CreatesDirectoryAndFile_IfTheyDoNotExist_AndReturnsFalse()
        {
            var reader = new IniReader(FullPath);

            Assert.That(Directory.Exists(TestDirectory), Is.False);
            Assert.That(File.Exists(FullPath), Is.False);

            var result = reader.Load();

            Assert.That(result, Is.False);
            Assert.That(Directory.Exists(TestDirectory), Is.True);
            Assert.That(File.Exists(FullPath), Is.True);
        }

        [Test]
        public void Load_HandlesEmptyLines()
        {
            var contents = "[Header]\nTestdata=aaaa\n\nTest2=bbbb";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var result = reader.Load();

            Assert.That(result, Is.True);
        }

        [Test]
        public void Load_HandlesCommentLines()
        {
            var contents = "[Header]\nTestdata=aaaa\n#this is a comment line\nTest2=bbbb";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var result = reader.Load();

            Assert.That(result, Is.True);
        }

        [Test]
        public void Load_HandlesCommentsOnHeaderLines()
        {
            var contents = "[Header]#some header comment\nTestdata=aaaa\n\n#this is a comment line\nTest2=bbbb";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var result = reader.Load();

            Assert.That(result, Is.True);
        }

        [Test]
        public void Load_HandlesCommentsOnDataLines()
        {
            var contents = "[Header]\nTestdata=aaaa #some other comment\n\n#this is a comment line\nTest2=bbbb";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var result = reader.Load();

            Assert.That(result, Is.True);
        }

        [Test]
        public void Load_HandlesMultipleEqualsOnDataLines()
        {
            var contents = "[Header]\nTestdata=aaaa=something #some other comment\n\n#this is a comment line\nTest2=bbbb";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var result = reader.Load();

            Assert.That(result, Is.True);
        }

        [Test]
        public void GetValue_String_FalseIfNonExistentSection()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            string value;
            var result = reader.GetValue("BadHeader", "Testdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_String_FalseIfNonExistentKey()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            string value;
            var result = reader.GetValue("Header", "BadTestdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_Int_FalseIfNonExistentSection()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            int value;
            var result = reader.GetValue("BadHeader", "Testdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_Int_FalseIfNonExistentKey()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            int value;
            var result = reader.GetValue("Header", "BadTestdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_Int_FalseIfNotAnInteger()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            int value;
            var result = reader.GetValue("Header", "Testdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_Bool_FalseIfNonExistentSection()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            bool value;
            var result = reader.GetValue("BadHeader", "Testdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_Bool_FalseIfNonExistentKey()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            bool value;
            var result = reader.GetValue("Header", "BadTestdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_Bool_FalseIfNotBoolean()
        {
            var contents = "[Header]\nTestdata=aaaa";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            bool value;
            var result = reader.GetValue("Header", "Testdata", out value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetValue_String_ReturnsValueAsString()
        {
            var contents = "[Header]\nTestdata=123";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            string value;
            var result = reader.GetValue("Header", "Testdata", out value);
            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("123"));
        }

        [Test]
        public void GetValue_Int_ReturnsValueAsInteger()
        {
            var contents = "[Header]\nTestdata=123";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            int value;
            var result = reader.GetValue("Header", "Testdata", out value);
            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(123));
        }

        [Test]
        public void GetValue_Bool_ReturnsValueAsBoolean()
        {
            var contents = "[Header]\nTestdata=true";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            bool value;
            var result = reader.GetValue("Header", "Testdata", out value);
            Assert.That(result, Is.True);
            Assert.That(value, Is.True);
        }

        [Test]
        public void GetValue_Bool_ConvertsBoolEquivalentsToTrue()
        {
            var contents = "[Header]\nItem1=yes\nItem2=1\nItem3=on";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            for (int i = 1; i <= 3; ++i)
            {
                bool value;
                var result = reader.GetValue("Header", "Item" + i, out value);
                Assert.That(result, Is.True);
                Assert.That(value, Is.True);
            }
        }

        [Test]
        public void GetValue_Bool_ConvertsBoolEquivalentsToFalse()
        {
            var contents = "[Header]\nItem1=no\nItem2=0\nItem3=off";
            CreateTestFileWithData(contents);

            var reader = new IniReader(FullPath);
            var loadResult = reader.Load();
            Assert.That(loadResult, Is.True);

            for (int i = 1; i <= 3; ++i)
            {
                bool value;
                var result = reader.GetValue("Header", "Item" + i, out value);
                Assert.That(result, Is.True);
                Assert.That(value, Is.False);
            }
        }

        private static void CreateTestFileWithData(string contents)
        {
            if (!Directory.Exists(TestDirectory))
                Directory.CreateDirectory(TestDirectory);

            File.WriteAllText(FullPath, contents);
        }
    }
}
