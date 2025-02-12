namespace AuthServer.IntegrationTests.Infrastructure;

public static class TestCollections
{
    [CollectionDefinition(nameof(Default), DisableParallelization = true)]
    public class Default : ICollectionFixture<ApplicationFactoryFixture>;
}