using EOLib.IO.Pub;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EOLib.IO.Test.Pub
{
    [TestFixture]
    public class BasePubFileTest_EIFImpl : BasePubFileTest<EIFFile, EIFRecord> { }

    [TestFixture]
    public class BasePubFileTest_ENFImpl : BasePubFileTest<ENFFile, ENFRecord> { }

    [TestFixture]
    public class BasePubFileTest_ESFImpl : BasePubFileTest<ESFFile, ESFRecord> { }

    [TestFixture]
    public class BasePubFileTest_ECFImpl : BasePubFileTest<ECFFile, ECFRecord> { }

    // These tests are run from the implementations
    [ExcludeFromCodeCoverage]
    public abstract class BasePubFileTest<T, U>
        where T : IPubFile<U>, new()
        where U : class, IPubRecord, new()
    {
        [Test]
        public void WithAddedRecord_AddsRecord()
        {
            var file = new T();
            var record = (U)new U().WithID(1).WithName("My record");

            var updatedFile = file.WithAddedRecord(record);

            Assert.That(file, Is.Empty);
            Assert.That(updatedFile, Has.Length.EqualTo(1));
            Assert.That(updatedFile, Has.Exactly(1).Items.EqualTo(record));
        }

        [Test]
        public void WithAddedRecord_DuplicateID_ThrowsArgumentException()
        {
            var file = new T();
            var record = (U)new U().WithID(1).WithName("My record");

            var updatedFile = file.WithAddedRecord(record);

            Assert.That(() => updatedFile.WithAddedRecord(record), Throws.ArgumentException);
        }

        [Test]
        public void WithAddedRecord_IDOutOfBounds_ThrowsArgumentException()
        {
            var file = new T();
            var record = (U)new U().WithID(400);

            Assert.That(() => file.WithAddedRecord(record), Throws.ArgumentException);
        }

        [Test]
        public void WithInsertedRecord_InsertsRecordAtPosition_SpecifiedByID()
        {
            IPubFile<U> file = new T();
            var record = (U)new U().WithID(1);

            file = file.WithAddedRecord(record);

            var updatedRecord = (U)record.WithName("updated");
            var updatedFile = file.WithInsertedRecord(updatedRecord);

            Assert.That(file, Has.Length.EqualTo(1));
            Assert.That(updatedFile, Has.Length.EqualTo(2));
            Assert.That(updatedFile[1].Name, Is.EqualTo("updated"));
        }

        [Test]
        public void WithInsertedRecord_UpdatesExistingRecordIDs()
        {
            IPubFile<U> file = new T();
            var record = (U)new U().WithID(1);

            file = file.WithAddedRecord(record);

            var updatedRecord = (U)record.WithName("updated");
            var updatedFile = file.WithInsertedRecord(updatedRecord)
                .WithInsertedRecord((U)updatedRecord.WithName("updated 2"))
                .WithInsertedRecord((U)updatedRecord.WithName("updated 3"));

            Assert.That(file, Has.Length.EqualTo(1));
            Assert.That(updatedFile, Has.Length.EqualTo(4));
            Assert.That(updatedFile[1], Has.Property("Name").EqualTo("updated 3").And.Property("ID").EqualTo(1));
            Assert.That(updatedFile[2], Has.Property("Name").EqualTo("updated 2").And.Property("ID").EqualTo(2));
            Assert.That(updatedFile[3], Has.Property("Name").EqualTo("updated").And.Property("ID").EqualTo(3));
            Assert.That(updatedFile[4], Has.Property("Name").EqualTo(string.Empty).And.Property("ID").EqualTo(4));
        }

        [Test]
        public void WithInsertedRecord_IDOutOfRange_ThrowsArgumentException()
        {
            IPubFile<U> file = new T();
            var record = (U)new U().WithID(2);

            Assert.That(() => file.WithInsertedRecord(record), Throws.ArgumentException);
        }

        [Test]
        public void WithUpdatedRecord_UpdatesRecordProperties_ByRecordID()
        {
            IPubFile<U> file = new T();
            var record = (U)new U().WithID(1);
            file = file.WithAddedRecord(record);

            var updatedRecord = (U)record.WithName("Some name");
            var updatedFile = file.WithUpdatedRecord(updatedRecord);

            Assert.That(updatedFile[1].Name, Is.EqualTo("Some name"));
        }

        [Test]
        public void WithUpdatedRecord_IDOutOfRange_ThrowsArgumentException()
        {
            IPubFile<U> file = new T();
            var record = (U)new U().WithID(1);
            file = file.WithAddedRecord(record);

            Assert.That(() => file.WithUpdatedRecord((U)record.WithID(2)), Throws.ArgumentException);
        }

        [Test]
        public void WithRemovedRecord_RemovesRecord()
        {
            var record = (U)new U().WithID(1).WithName("My record");

            var file = new T().WithAddedRecord(record);
            var updatedFile = file.WithRemovedRecord(record);

            Assert.That(updatedFile, Is.Empty);
            Assert.That(file, Has.Length.EqualTo(1));
        }

        [Test]
        public void WithRemovedRecord_RemovesRecord_UpdatesIDs()
        {
            var record = (U)new U().WithID(1).WithName("My record");
            var record2 = (U)new U().WithID(2).WithName("My record 2");
            var record3 = (U)new U().WithID(3).WithName("My record 3");
            var record4 = (U)new U().WithID(4).WithName("My record 4");

            var file = new T().WithAddedRecord(record)
                .WithAddedRecord(record2)
                .WithAddedRecord(record3)
                .WithAddedRecord(record4);
            var updatedFile = file.WithRemovedRecord(record);

            Assert.That(file, Has.Length.EqualTo(4));
            Assert.That(updatedFile, Has.Length.EqualTo(3));

            Assert.That(updatedFile[1].ID, Is.EqualTo(1));

            Assert.That(updatedFile, Has.Exactly(1).Items.EqualTo(record2.WithID(1)));
            Assert.That(updatedFile, Has.Exactly(1).Items.EqualTo(record3.WithID(2)));
            Assert.That(updatedFile, Has.Exactly(1).Items.EqualTo(record4.WithID(3)));
            Assert.That(updatedFile, Has.None.With.Property("ID").EqualTo(4));
        }

        [Test]
        public void WithRemovedRecord_IDOutOfrange_ThrowsArgumentException()
        {
            IPubFile<U> file = new T();
            var record = (U)new U().WithID(1);
            file = file.WithAddedRecord(record);

            Assert.That(() => file.WithRemovedRecord((U)record.WithID(2)), Throws.ArgumentException);
        }
    }
}