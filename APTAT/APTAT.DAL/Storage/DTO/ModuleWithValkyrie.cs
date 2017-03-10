using APTAT.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.DAL.Storage.DTO
{
    public class ModuleWithValkyrie : Module
    {
        public ValkyrieFileInfo ValkyrieDetails { get; set; }
    }
}
