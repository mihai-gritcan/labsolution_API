using System;

namespace LabSolution.GovSync
{
    public class TestPushModel
    {
        public PersonInfoModel PersonInfo { get; set; }
        public SampleInfoModel SampleInfo { get; set; }
        public VaccinationInfoModel VaccinationInfo { get; set; }
        public DateTime CaseStartDate { get; set; }
    }

    public class PersonInfoModel
    {
        public bool IsResident { get; set; }
        public string IdentityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDay { get; set; }
        public string PhoneNumber { get; set; }
        public AddressModel Address { get; set; }
        public WorkingInfoModel WorkingInfo { get; set; }
        public string Gender { get; set; } // Male, Female
    }

    public class AddressModel
    {
        public string Municipality { get; set; }
        public string Settlement { get; set; }
        public string Street { get; set; }
    }

    public class WorkingInfoModel
    {
        public string Position { get; set; }
    }

    public class SampleInfoModel
    {
        public string LaboratoryId { get; set; }
        public string LaboratoryOfficeId { get; set; }
        public string LaboratoryTestNumber { get; set; }
        public string SampleType { get; set; } // PCR, AntiGen 
        public DateTime SampleCollectionAt { get; set; }
        public string SampleResult { get; set; } // Positive, Negative
        public string TestDeviceIdentifier { get; set; } // should have data for Antigen tests only
    }

    public class VaccinationInfoModel
    {
        public bool IsVaccinated { get; set; }
        public string VaccineName { get; set; }
    }
}


/*
 {
"personInfo": {
"isResident": true,
"identityNumber": "string",
"firstName": "string",
"lastName": "string",
"age": 0,
"birthDay": "2022-01-19T22:09:36.270Z",
"documentNumber": "string",
"countryIssueDocument": "st",
"phoneNumber": "string",
"address": {
  "municipality": "string",
  "settlement": "string",
  "street": "string"
},
"workingInfo": {
  "position": "string",
  "place": "string",
  "isMedicalWorker": true,
  "medicalWorkingPosition": "Medic",
  "medicalWorkingService": "AMS"
},
"isPregnant": true,
"pregnancyStage": "I",
"gender": "Male",
"settlementType": "Urban"
},
"sampleInfo": {
"laboratoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
"laboratoryOfficeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
"labImspInvestigation": "string",
"labImspCollection": "string",
"laboratoryTestNumber": "string",
"sampleType": "PCR",
"manufacturerAndTestName": "string",
"testDeviceIdentifier": "string",
"sampleCollectionType": "NasopharyngealAndPharyngealSwab",
"sampleCollectionAt": "2022-01-19T22:09:36.270Z",
"sampleReceivedAt": "2022-01-19T22:09:36.270Z",
"sampleInvestigationAt": "2022-01-19T22:09:36.270Z",
"sampleResult": "Positive"
},
"visitedCountry": "st",
"returnDate": "2022-01-19T22:09:36.270Z",
"caseStartDate": "2022-01-19T22:09:36.270Z",
"caseProvenance": "Local",
"isConfirmedContact": true,
"vaccinationInfo": {
"isVaccinated": true,
"lastAdministration": "2022-01-19T22:09:36.270Z",
"vaccineName": "string",
"stage": "string",
"isCompleted": true
},
"notes": "string",
"ipAddress": "string"
}
 */
