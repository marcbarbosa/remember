﻿using Remember.Interfaces;
using System;
using System.Runtime.Caching;
using Xunit;

namespace Remember.Tests
{
    public class RememberTests
    {
        public class Integration : IDisposable
        {
            private readonly IRemember remember = Remember.Instance;

            [Fact]
            public void CallingAllMethods()
            {
                var cacheKey = "user@domain.com";
                var expectedInnerUser = new User { Email = "inneruser@domain.com", Password = "Akcjhs876482u3jhKJc8d6f87234j2Bkjshdi==" };
                var expectedUser = new User { Email = cacheKey, Password = "Akcjhs876482u3jhKJc8d6f87234j2Bkjshdi==", InnerUser = expectedInnerUser };

                var actual = remember.GetAsync<User>(cacheKey).Result;
                Assert.Null(actual);

                remember.SaveAsync<User>(cacheKey, expectedUser).Wait();
                actual = remember.GetAsync<User>(cacheKey).Result;
                Assert.NotNull(actual);
                Assert.NotNull(actual.InnerUser);
                Assert.Equal(expectedUser.Email, actual.Email);
                Assert.Equal(expectedUser.Password, actual.Password);
                Assert.Equal(expectedInnerUser.Email, actual.InnerUser.Email);
                Assert.Equal(expectedInnerUser.Password, actual.InnerUser.Password);

                MemoryCache.Default.Remove(cacheKey);
                actual = remember.GetAsync<User>(cacheKey).Result;
                Assert.NotNull(actual);

                remember.DeleteAsync(cacheKey).Wait();
                actual = remember.GetAsync<User>(cacheKey).Result;
                Assert.Null(actual);
            }

            public class User
            {
                public string Email { get; set; }

                public string Password { get; set; }

                public User InnerUser { get; set; }
            }

            public void Dispose()
            {
                remember.Dispose();
            }
        }
    }
}
