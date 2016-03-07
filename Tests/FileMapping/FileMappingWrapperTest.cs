using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using com.veeam.Compresser.FileMapping;

namespace com.veeam.Compresser.Tests.FileMapping
{
    [TestFixture]
    class FileMappingWrapperTest
    {
        [Test]
        public void CreateFromFile_FileMappingHandleIsValid()
        {
            // arrange
            string path = @"../../Data/testdata.txt"; // source file;

            // act
            bool handleWasInitialized = false;
            FileMappingWrapper hFileMapping;
            using (hFileMapping = FileMappingWrapper.CreateFromFile(path))
            {
                if (hFileMapping.FileMappingHandle != null
                    && !hFileMapping.FileMappingHandle.IsInvalid)
                    handleWasInitialized = true;
            }

            // assert
            Assert.IsTrue(handleWasInitialized, "_01");
            Assert.IsTrue(hFileMapping.FileMappingHandle != null, "_02");
            Assert.IsTrue(hFileMapping.FileMappingHandle.IsInvalid, "_03");
        }

        [Test]
        public void CreateViewAccessor_FileMappingViewHandleIsValid()
        {
            // arrange
            string path = @"../../Data/testdata.txt"; // source file;

            // act
            bool handleWasInitialized = false;
            FileMappingViewAccessor accessor;
            using (var hFileMapping = FileMappingWrapper.CreateFromFile(path))
            {
                using (accessor = hFileMapping.CreateViewAccessor(0, 0))
                {
                    if (accessor.FileMappingViewHandle != null
                    && !accessor.FileMappingViewHandle.IsInvalid)
                        handleWasInitialized = true;
                }
            }

            // assert
            Assert.IsTrue(handleWasInitialized, "_01");
            Assert.IsTrue(accessor.FileMappingViewHandle != null, "_02");
            Assert.IsTrue(accessor.FileMappingViewHandle.IsInvalid, "_03");
        }
    }
}
