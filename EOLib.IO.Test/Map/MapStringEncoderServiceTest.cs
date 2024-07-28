using EOLib.IO.Services;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace EOLib.IO.Test.Map;

[TestFixture, ExcludeFromCodeCoverage]
public class MapStringEncoderServiceTest
{
    private IMapStringEncoderService _service;

    [SetUp]
    public void SetUp()
    {
        _service = new MapStringEncoderService();
    }

    [Test]
    public void EncodeThenDecode_ReturnsOriginalString()
    {
        const string expected = "Test map string to encode";

        var bytes = _service.EncodeMapString(expected, expected.Length);
        var actual = _service.DecodeMapString(bytes);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void EncodeString_ReturnsExpectedBytes_FromKnownString()
    {
        var name = "Aeven" + Encoding.ASCII.GetString(Enumerable.Repeat((byte)0, 19).ToArray());
        var expectedBytes = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 49, 104, 41, 104, 94
        };

        var actualBytes = _service.EncodeMapString(name, name.Length);

        CollectionAssert.AreEqual(expectedBytes, actualBytes);
    }

    [Test]
    public void DecodeString_ReturnsExpectedString_FromKnownBytes()
    {
        const string expected = "Aeven";

        var bytes = new byte[] { 49, 104, 41, 104, 94 };
        var fullBytes = Enumerable.Repeat((byte)255, 24).ToArray();
        Array.Copy(bytes, 0, fullBytes, fullBytes.Length - bytes.Length, bytes.Length);

        var actual = _service.DecodeMapString(fullBytes);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void EncodeString_InvalidLength_Throws()
    {
        Assert.That(() => _service.EncodeMapString("123", 0), Throws.ArgumentException);
    }

    [Test]
    public void EncodeString_ExtraLength_PadsData()
    {
        const string TestString = "12345";
        const int LengthWithPadding = 8;

        var actual = _service.EncodeMapString(TestString, LengthWithPadding);

        Assert.That(actual, Has.Length.EqualTo(LengthWithPadding));

        int i = 0;
        for (; i < LengthWithPadding - TestString.Length; i++)
            Assert.That(actual[i], Is.EqualTo((byte)0xFF));
        Assert.That(actual[i], Is.Not.EqualTo((byte)0xFF));
    }

    [Test]
    public void EncodeString_ExtraLength_DecodesToExpectedValue()
    {
        const string TestString = "12345";
        const int LengthWithPadding = 8;

        var encoded = _service.EncodeMapString(TestString, LengthWithPadding);
        var original = _service.DecodeMapString(encoded);

        Assert.That(original, Is.EqualTo(TestString));
    }
}