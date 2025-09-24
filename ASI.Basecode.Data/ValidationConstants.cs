// <copyright file="ValidationConstants.cs" company="Alliance Software Inc">
// Copyright (c) Alliance Software Inc. All rights reserved.
// </copyright>

namespace ASI.Basecode.Data
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// Contains constants for validation in the data layer.
    /// </summary>
    public static class ValidationConstants
    {
        /// <summary>
        /// Contains status codes for CRUD operations.
        /// </summary>
        public static class InputStatus
        {
            /// <summary>
            /// Represents a successful operation.
            /// </summary>
            public static readonly int Valid = 1;

            /// <summary>
            /// Represents a record that does not exist.
            /// </summary>
            public static readonly int DoesNotExist;

            /// <summary>
            /// Represents a failed process.
            /// </summary>
            public static readonly int Invalid = -1;

            /// <summary>
            /// Represents a duplicate record.
            /// </summary>
            public static readonly int DuplicateExist = -2;

            /// <summary>
            /// Represents a record that is currently in use.
            /// </summary>
            public static readonly int InUse = -3;

            /// <summary>
            /// Represents a admission period that is already close.
            /// </summary>
            public static readonly int AdmissionPeriodIsClose = -4;

            /// <summary>
            /// Represents a duplicate record.
            /// </summary>
            public static readonly int DuplicateGradeRangeExist = -5;
        }

        public static class OutputStatus
        {
            /// <summary>
            /// Represents a successful creation.
            /// </summary>
            public static readonly int Created = 3;

            /// <summary>
            /// Represents a successful update.
            /// </summary>
            public static readonly int Updated = 2;

            /// <summary>
            /// Represents a succesful operation.
            /// </summary>
            public static readonly int Success = 1;

            /// <summary>
            /// Represents a failure because record does not exist.
            /// </summary>
            public static readonly int DoesNotExist;

            /// <summary>
            /// Represents a failed process.
            /// </summary>
            public static readonly int ProcessFail = -1;

            /// <summary>
            /// Represents a failure because a duplicate record exists.
            /// </summary>
            public static readonly int DuplicateExist = -2;

            /// <summary>
            /// Represents a failure because record is currently in use.
            /// </summary>
            public static readonly int InUse = -3;

            /// <summary>
            /// Represents a failure because record is not added.
            /// </summary>
            public static readonly int NotCreated = -4;

            /// <summary>
            /// Represents a failure because confirmation is needed.
            /// </summary>
            public static readonly int NeedConfirmation = -5;

            /// <summary>
            /// Represents a failure because record is not updated.
            /// </summary>
            public static readonly int NotUpdated = -6;

            /// <summary>
            /// Represents a failure because record is not deleted.
            /// </summary>
            public static readonly int NotDeleted = -7;

            /// <summary>
            /// Represents a failure because a field is missing.
            /// </summary>
            public static readonly int MissingField = -8;

            /// <summary>
            /// Represents a failure because a field is invalid.
            /// </summary>
            public static readonly int InvalidField = -9;

            /// <summary>
            /// Represents a success duplication.
            /// </summary>
            public static readonly int Duplicated = -11;

            /// <summary>
            /// Represents a failed duplication.
            /// </summary>
            public static readonly int NotDuplicated = -12;
        }

        /// <summary>
        /// Contains status Log types per transaction.
        /// </summary>
        public static class LogTypes
        {
            public static readonly string ADD = "Add";
            public static readonly string EDIT = "Edit";
            public static readonly string DELETE = "Delete";
        }
    }
}
