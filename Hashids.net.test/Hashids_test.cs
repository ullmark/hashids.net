using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Hashids.net.test
{
	public class Hashids_test
	{
		Hashids hashids;

		public Hashids_test()
		{
			hashids = new Hashids("this is my salt");
		}

		[Fact]
		void it_has_a_default_salt()
		{
			new Hashids().Encrypt(1,2,3).Should().Be("katKSA");
		}

		[Fact]
		void it_has_the_correct_salt()
		{
			hashids.Salt.Should().Be("this is my salt");
		}

		[Fact]
		void it_defaults_to_a_min_length_of_0()
		{
			hashids.MinHashLength.Should().Be(0);
		}

		[Fact]
		void it_encrypts_a_single_number()
		{
			hashids.Encrypt(12345).Should().Be("ryBo");
			hashids.Encrypt(1).Should().Be("LX");
			hashids.Encrypt(22).Should().Be("5B");
			hashids.Encrypt(333).Should().Be("o49");
			hashids.Encrypt(9999).Should().Be("GKnB");
		}

		[Fact]
		void it_encrypts_a_list_of_numbers()
		{
			//hashids.Encrypt(683, 94108, 123, 5).Should().Be("zBphL54nuMyu5");
			hashids.Encrypt(1, 2, 3).Should().Be("eGtrS8");
		}

		[Fact]
		void it_decrypts_an_ecryptet_number()
		{

		}
	}
}
