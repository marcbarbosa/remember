using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Remember.Tests
{
    public class RememberTests
    {
        public class SaveAsync
        {
            [Fact]
            public async void Save()
            {
                var innerUser = new User { Email = "inneruser@gmail.com", Password = "Akcjhs876482u3jhKJc8d6f87234j2Bkjshdi==" };
                var expected = new User { Email = "user@domain.com", Password = "Akcjhs876482u3jhKJc8d6f87234j2Bkjshdi==", InnerUser = innerUser };
                
                await Remember.Instance.SaveAsync<User>(expected.Email, expected);

                var actual = Remember.Instance.GetAsync<User>(expected.Email).Result;

                Assert.Equal(expected.Password, actual.Password);
            }

            public class User
            {
                public string Email { get; set; }

                public string Password { get; set; }

                public User InnerUser { get; set; }
            }
        }
    }
}
