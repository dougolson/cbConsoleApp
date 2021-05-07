﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CoinbasePro.Network.HttpClient;
using CoinbasePro.Services.Withdrawals;
using CoinbasePro.Services.Withdrawals.Models;
using CoinbasePro.Services.Withdrawals.Models.Responses;
using CoinbasePro.Shared.Types;
using CoinbasePro.Specs.JsonFixtures.Withdrawals;
using Machine.Fakes;
using Machine.Specifications;

namespace CoinbasePro.Specs.Services.Withdrawals
{
    [Subject("WithdrawalsService")]
    public class WithdrawalsServiceSpecs : WithSubject<WithdrawalsService>
    {
        static WithdrawalResponse withdrawals_response;

        static CoinbaseResponse coinbase_response;

        static CryptoResponse crypto_response;

        Establish context = () =>
            The<IHttpClient>().WhenToldTo(p => p.SendAsync(Param.IsAny<HttpRequestMessage>()))
                .Return(Task.FromResult(new HttpResponseMessage()));

        class when_requesting_a_withdrawal
        {
            Establish context = () =>
                The<IHttpClient>().WhenToldTo(p => p.ReadAsStringAsync(Param.IsAny<HttpResponseMessage>()))
                    .Return(Task.FromResult(WithdrawalsResponseFixture.Create()));

            Because of = () =>
                withdrawals_response = Subject.WithdrawFundsAsync("593533d2-ff31-46e0-b22e-ca754147a96a", 10, Currency.USD).Result;

            It should_return_a_response = () =>
                withdrawals_response.ShouldNotBeNull();

            It should_return_a_correct_response = () =>
            {
                withdrawals_response.Id.ShouldEqual(new Guid("593533d2-ff31-46e0-b22e-ca754147a96a"));
                withdrawals_response.Amount.ShouldEqual(10.00M);
                withdrawals_response.Currency.ShouldEqual(Currency.USD);
                withdrawals_response.PayoutAt.ShouldEqual(new DateTime(2016, 12, 9));
            };
        }

        class when_requesting_coinbase_withdrawal
        {
            Establish context = () =>
                The<IHttpClient>().WhenToldTo(p => p.ReadAsStringAsync(Param.IsAny<HttpResponseMessage>())).Return(Task.FromResult(CoinbaseWithdrawalResponseFixture.Create()));

            Because of = () =>
                coinbase_response = Subject.WithdrawToCoinbaseAsync("593533d2-ff31-46e0-b22e-ca754147a96a", 10, Currency.BTC).Result;

            It should_return_a_response = () =>
                coinbase_response.ShouldNotBeNull();

            It should_return_a_correct_response = () =>
            {
                coinbase_response.Id.ShouldEqual(new Guid("593533d2-ff31-46e0-b22e-ca754147a96a"));
                coinbase_response.Amount.ShouldEqual(10.00M);
                coinbase_response.Currency.ShouldEqual(Currency.BTC);
            };
        }

        class when_requesting_crypto_withdrawal
        {
            Establish context = () =>
                The<IHttpClient>().WhenToldTo(p => p.ReadAsStringAsync(Param.IsAny<HttpResponseMessage>())).Return(Task.FromResult(CryptoWithdrawalResponseFixture.Create()));

            Because of = () =>
                crypto_response = Subject.WithdrawToCryptoAsync("0x5ad5769cd04681FeD900BCE3DDc877B50E83d469", 10.0M, Currency.BTC).Result;

            It should_return_a_response = () =>
                crypto_response.ShouldNotBeNull();

            It should_return_a_correct_response = () =>
            {
                crypto_response.Id.ShouldNotBeTheSameAs(Guid.Empty);
                crypto_response.Amount.ShouldEqual(10.00M);
                crypto_response.Currency.ShouldEqual(Currency.BTC);
            };
        }

        class when_requesting_all_withdrawals
        {
            static IEnumerable<Transfer> all_withdrawals_response;

            Establish context = () =>
                The<IHttpClient>().WhenToldTo(p => p.ReadAsStringAsync(Param.IsAny<HttpResponseMessage>())).Return(Task.FromResult(CryptoWithdrawalResponseFixture.CreateAll()));

            Because of = () =>
                all_withdrawals_response = Subject.GetAllWithdrawals().Result;

            It should_return_a_response = () =>
                all_withdrawals_response.ShouldNotBeNull();

            It should_return_a_correct_response = () =>
            {
                all_withdrawals_response.Count().ShouldEqual(1);
                all_withdrawals_response.First().Id.ShouldEqual(new Guid("6b09bf5e-c94c-405b-b7dc-ad2b27749ce5"));
                all_withdrawals_response.First().Amount.ShouldEqual(22.00M);
                all_withdrawals_response.First().Details.DestinationTag.ShouldEqual("567148403");
                all_withdrawals_response.First().Details.Fee.ShouldEqual(.01M);
                all_withdrawals_response.First().Details.Subtotal.ShouldEqual(22.0M);
            };
        }

        class when_requesting_withdrawal_by_transfer_id
        {
            static Transfer withdrawal_response;

            Establish context = () =>
                The<IHttpClient>().WhenToldTo(p => p.ReadAsStringAsync(Param.IsAny<HttpResponseMessage>())).Return(Task.FromResult(CryptoWithdrawalResponseFixture.CreateTransferById()));

            Because of = () =>
                withdrawal_response = Subject.GetWithdrawalById("1").Result;

            It should_return_a_response = () =>
                withdrawal_response.ShouldNotBeNull();

            It should_return_a_correct_response = () =>
            {
                withdrawal_response.Id.ShouldEqual(new Guid("6b09bf5e-c94c-405b-b7dc-ad2b27749ce5"));
                withdrawal_response.Amount.ShouldEqual(22.00M);
                withdrawal_response.Details.DestinationTag.ShouldEqual("567148403");
                withdrawal_response.Details.Fee.ShouldEqual(.01M);
                withdrawal_response.Details.Subtotal.ShouldEqual(22.0M);
            };
        }

        class when_requesting_withdrawal_fee_estimate
        {
            static FeeEstimateResponse response;

            Establish context = () =>
                The<IHttpClient>().WhenToldTo(p => p.ReadAsStringAsync(Param.IsAny<HttpResponseMessage>())).Return(Task.FromResult(CryptoWithdrawalResponseFixture.GetFeeEstimateResponse()));

            Because of = () =>
                response = Subject.GetFeeEstimateAsync(Currency.ALGO, "crypto_address_123").Result;

            It should_return_a_response = () =>
                response.ShouldNotBeNull();

            It should_return_a_correct_response = () =>
                response.Fee.ShouldEqual(0.01);
        }
    }
}
