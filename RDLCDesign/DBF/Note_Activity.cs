namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Note_Activity
    {
        public int Id { get; set; }

        public int? NoteId { get; set; }

        public int? ActivityId { get; set; }

        public string AnswerClient { get; set; }

        public string AnswerFacilitator { get; set; }
    }
}
