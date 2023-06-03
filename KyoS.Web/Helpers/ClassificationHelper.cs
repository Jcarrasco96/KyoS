using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public class ClassificationHelper : IClassificationHelper
    {
        private readonly DataContext _context;
        public ClassificationHelper(DataContext context)
        {
            _context = context;
        }
        public async Task CheckClassificationAsync(string classification)
        {
            bool exist = _context.Classifications.Any(c => c.Name == classification);
            if (!exist)
            {
                ClassificationEntity model = new ClassificationEntity()
                {
                    Name = classification
                };
                _context.Classifications.Add(model);
                try
                {
                    await _context.SaveChangesAsync();
                }
                finally
                {}
            }
        }
    }
}
