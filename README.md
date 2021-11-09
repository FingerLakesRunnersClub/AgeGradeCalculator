# Age Grade Calculator

A .NET implementation of USATF's Age Grade metric using Alan Jones' official tables (<https://github.com/AlanLyttonJones/Age-Grade-Tables>)

[![NuGet version](https://img.shields.io/nuget/v/AgeGradeCalculator?logo=nuget&label=Install)](https://nuget.org/packages/AgeGradeCalculator)
[![CI](https://github.com/FingerLakesRunnersClub/AgeGradeCalculator/actions/workflows/CI.yml/badge.svg)](https://github.com/FingerLakesRunnersClub/AgeGradeCalculator/actions/workflows/CI.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=FingerLakesRunnersClub_AgeGradeCalculator&metric=coverage)](https://sonarcloud.io/summary/new_code?id=FingerLakesRunnersClub_AgeGradeCalculator)

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=FingerLakesRunnersClub_AgeGradeCalculator&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=FingerLakesRunnersClub_AgeGradeCalculator)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=FingerLakesRunnersClub_AgeGradeCalculator&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=FingerLakesRunnersClub_AgeGradeCalculator)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=FingerLakesRunnersClub_AgeGradeCalculator&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=FingerLakesRunnersClub_AgeGradeCalculator)

## Requirements

A .NET runtime compatible with .NET Standard 2.0

## Installation

Simply add `AgeGradeCalculator` to your project via NuGet

## Usage

Call the static method with the appropriate parameters to receive a double representing the provided athlete's age grade percent:

```c#
var ageGrade1 = AgeGradeCalculator.GetAgeGrade(Category.M, 36, 10_000, TimeSpan.Parse("39:26.5"));
var ageGrade2 = AgeGradeCalculator.GetAgeGrade(Category.F, 29, 1609.3, TimeSpan.Parse("4:30.2"));
```
