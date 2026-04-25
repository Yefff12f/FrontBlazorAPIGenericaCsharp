using Microsoft.AspNetCore.Components;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    public class RolePermissionService
    {
        private readonly AuthenticationService _authService;

        private static readonly HashSet<string> InvestigacionPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/grupos",
            "/lineas",
            "/semilleros"
        };

        private static readonly HashSet<string> InvestigacionIntermediaPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/semillero-lineas",
            "/participa-semilleros",
            "/participa-grupos",
            "/grupo-lineas",
            "/ac-lineas",
            "/as-lineas",
            "/ods-lineas"
        };

        private static readonly HashSet<string> CaracterizacionPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/area-conocimiento",
            "/area-aplicacion",
            "/objetivos-ds"
        };

        private static readonly HashSet<string> ConocimientoPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/proyectos",
            "/productos"
        };

        private static readonly HashSet<string> ConocimientoIntermediaPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/desarrolla",
            "/ac-proyectos",
            "/aliado-proyectos",
            "/aa-proyectos",
            "/terminos-clave",
            "/palabras-clave",
            "/proyectos-linea",
            "/ods-proyectos",
            "/docente-producto",
            "/proyecto-productos",
            "/tipo-producto"
        };

        private static readonly HashSet<string> CurricularPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/universidades",
            "/facultades",
            "/programas",
            "/acreditaciones",
            "/premios",
            "/aspectos-normativos",
            "/car-innovaciones",
            "/practicas-estrategias",
            "/enfoques",
            "/aliados",
            "/pasantias",
            "/registros-calificados",
            "/actvs-academicas",
            "/alianzas",
            "/docentes-departamentos"
        };

        private static readonly HashSet<string> CurricularIntermediaPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/programas-ci",
            "/an-programas",
            "/programas-ac",
            "/programas-pe",
            "/enfoques-rc",
            "/aa-rcs"
        };

        private static readonly HashSet<string> ProfesionalPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/docentes",
            "/redes",
            "/reconocimientos",
            "/experiecia",
            "/evaluaciones-docente",
            "/estudios-realizados",
            "/becas",
            "/apoyos-profesoral"
        };

        private static readonly HashSet<string> ProfesionalIntermediaPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/red-docentes",
            "/estudios-ac",
            "/intereses-futuros"
        };

        private static readonly HashSet<string> AdminPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/usuarios"
        };

        private static readonly HashSet<string> InvestigacionTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "grupo_investigacion",
            "linea_investigacion",
            "semillero"
        };

        private static readonly HashSet<string> InvestigacionIntermediaTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "semillero_linea",
            "participa_semillero",
            "participa_grupo",
            "grupo_linea",
            "ac_linea",
            "as_linea",
            "ods_linea"
        };

        private static readonly HashSet<string> CaracterizacionTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "area_conocimiento",
            "area_aplicacion",
            "objetivos_desarrollo_sostenible"
        };

        private static readonly HashSet<string> ConocimientoTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "proyecto",
            "producto"
        };

        private static readonly HashSet<string> ConocimientoIntermediaTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "desarrolla",
            "ac_proyecto",
            "aliado_proyecto",
            "aa_proyecto",
            "termino_clave",
            "palabras_clave",
            "proyecto_linea",
            "ods_proyecto",
            "docente_producto",
            "tipo_producto"
        };

        private static readonly HashSet<string> CurricularTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "universidad",
            "facultad",
            "programa",
            "acreditacion",
            "premio",
            "aspecto_normativo",
            "car_innovacion",
            "practica_estrategia",
            "enfoque",
            "aliado",
            "pasantia",
            "registro_calificado",
            "actv_academica",
            "alianza",
            "docente_departamento"
        };

        private static readonly HashSet<string> CurricularIntermediaTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "programa_ci",
            "an_programa",
            "programa_ac",
            "programa_pe",
            "enfoque_rc",
            "aa_rc"
        };

        private static readonly HashSet<string> ProfesionalTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "docente",
            "red",
            "reconocimiento",
            "experiecia",
            "evaluacion_docente",
            "estudios_realizados",
            "beca",
            "apoyo_profesoral"
        };

        private static readonly HashSet<string> ProfesionalIntermediaTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "red_docente",
            "estudio_ac",
            "intereses_futuros"
        };

        public RolePermissionService(AuthenticationService authService)
        {
            _authService = authService;
        }

        public string GetRole()
        {
            return (_authService.GetRol() ?? string.Empty).Trim().ToLowerInvariant();
        }

        public bool CanAccessPath(string path)
        {
            path = NormalizePath(path);

            if (AdminPaths.Contains(path))
            {
                return GetRole() == "admin";
            }

            var module = GetModuleByPath(path);
            if (module == ModuleType.Public)
            {
                return true;
            }

            var role = GetRole();
            if (string.IsNullOrWhiteSpace(role))
            {
                return false;
            }

            if (role == "admin")
            {
                return true;
            }

            if (IsIntermediatePath(path))
            {
                return false;
            }

            return module switch
            {
                ModuleType.Profesional => role is "docente" or "coordinador" or "lector",
                ModuleType.Investigacion => role is "docente" or "coordinador" or "lector",
                ModuleType.Curricular => role is "docente" or "coordinador" or "lector",
                ModuleType.Conocimiento => role is "docente" or "coordinador" or "lector",
                _ => false
            };
        }

        public UiPermissions GetUiPermissionsForPath(string path)
        {
            path = NormalizePath(path);
            var module = GetModuleByPath(path);
            var role = GetRole();

            if (AdminPaths.Contains(path))
            {
                return role == "admin"
                    ? UiPermissions.FullAccess()
                    : UiPermissions.NoAccess();
            }

            if (module == ModuleType.Public || role == "admin")
            {
                return UiPermissions.FullAccess();
            }

            return module switch
            {
                ModuleType.Profesional => role switch
                {
                    "docente" => new UiPermissions(true, true, true, true, false),
                    "coordinador" => UiPermissions.ReadOnly(),
                    "lector" => UiPermissions.ReadOnly(),
                    _ => UiPermissions.NoAccess()
                },
                ModuleType.Investigacion => role switch
                {
                    "docente" => new UiPermissions(true, false, false, true, false),
                    "coordinador" => new UiPermissions(true, true, false, true, false),
                    "lector" => UiPermissions.ReadOnly(),
                    _ => UiPermissions.NoAccess()
                },
                ModuleType.Curricular => role switch
                {
                    "coordinador" => new UiPermissions(true, true, true, true, false),
                    "docente" => UiPermissions.ReadOnly(),
                    "lector" => UiPermissions.ReadOnly(),
                    _ => UiPermissions.NoAccess()
                },
                ModuleType.Conocimiento => role switch
                {
                    "docente" => new UiPermissions(true, false, false, true, false),
                    "coordinador" => new UiPermissions(true, true, false, true, false),
                    "lector" => UiPermissions.ReadOnly(),
                    _ => UiPermissions.NoAccess()
                },
                _ => UiPermissions.NoAccess()
            };
        }

        public bool CanCreateTable(string tabla)
        {
            if (IsIntermediateTable(tabla))
            {
                return GetRole() == "admin";
            }

            return GetUiPermissionsForModule(GetModuleByTable(tabla)).CanCreate;
        }

        public bool CanEditTable(string tabla)
        {
            if (IsIntermediateTable(tabla))
            {
                return GetRole() == "admin";
            }

            return GetUiPermissionsForModule(GetModuleByTable(tabla)).CanEdit;
        }

        public bool CanDeleteTable(string tabla)
        {
            if (IsIntermediateTable(tabla))
            {
                return GetRole() == "admin";
            }

            return GetUiPermissionsForModule(GetModuleByTable(tabla)).CanDelete;
        }

        public bool CanManageIntermediateTable(string tabla)
        {
            if (!IsIntermediateTable(tabla))
            {
                return true;
            }

            return GetRole() == "admin";
        }

        public int? GetDocenteId()
        {
            if (GetRole() != "docente")
            {
                return null;
            }

            var usuario = _authService.GetUsuario();
            if (int.TryParse(usuario, out var docenteId))
            {
                return docenteId;
            }

            return null;
        }

        public bool IsOwnDocenteRecord(string nombreClave, string valorClave, Dictionary<string, object?>? datos = null)
        {
            var docenteId = GetDocenteId();
            if (docenteId == null)
            {
                return true;
            }

            if (string.Equals(nombreClave, "cedula", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(nombreClave, "docente", StringComparison.OrdinalIgnoreCase))
            {
                return valorClave == docenteId.Value.ToString();
            }

            if (datos != null)
            {
                if (TryGetNumericValue(datos, "cedula", out var cedula))
                {
                    return cedula == docenteId.Value;
                }

                if (TryGetNumericValue(datos, "docente", out var docente))
                {
                    return docente == docenteId.Value;
                }
            }

            return true;
        }

        public bool IsRecordOwnedByCurrentDocente(Dictionary<string, object?> registro)
        {
            var docenteId = GetDocenteId();
            if (docenteId == null)
            {
                return true;
            }

            if (TryGetNumericValue(registro, "cedula", out var cedula))
            {
                return cedula == docenteId.Value;
            }

            if (TryGetNumericValue(registro, "docente", out var docente))
            {
                return docente == docenteId.Value;
            }

            return false;
        }

        private UiPermissions GetUiPermissionsForModule(ModuleType module)
        {
            return module switch
            {
                ModuleType.Profesional => GetUiPermissionsForPath("/docentes"),
                ModuleType.Investigacion => GetUiPermissionsForPath("/grupos"),
                ModuleType.Curricular => GetUiPermissionsForPath("/programas"),
                ModuleType.Conocimiento => GetUiPermissionsForPath("/proyectos"),
                _ => UiPermissions.NoAccess()
            };
        }

        private static bool TryGetNumericValue(Dictionary<string, object?> datos, string key, out int value)
        {
            value = 0;
            if (!datos.TryGetValue(key, out var raw) || raw == null)
            {
                return false;
            }

            return int.TryParse(raw.ToString(), out value);
        }

        private ModuleType GetModuleByPath(string path)
        {
            path = NormalizePath(path);

            if (string.IsNullOrEmpty(path) || path == "/" || path == "/login")
            {
                return ModuleType.Public;
            }

            if (InvestigacionPaths.Contains(path) || InvestigacionIntermediaPaths.Contains(path))
            {
                return ModuleType.Investigacion;
            }

            if (CaracterizacionPaths.Contains(path) || ConocimientoPaths.Contains(path) || ConocimientoIntermediaPaths.Contains(path))
            {
                return ModuleType.Conocimiento;
            }

            if (CurricularPaths.Contains(path) || CurricularIntermediaPaths.Contains(path))
            {
                return ModuleType.Curricular;
            }

            if (ProfesionalPaths.Contains(path) || ProfesionalIntermediaPaths.Contains(path))
            {
                return ModuleType.Profesional;
            }

            return ModuleType.Public;
        }

        private ModuleType GetModuleByTable(string tabla)
        {
            if (InvestigacionTables.Contains(tabla) || InvestigacionIntermediaTables.Contains(tabla))
            {
                return ModuleType.Investigacion;
            }

            if (CaracterizacionTables.Contains(tabla) || ConocimientoTables.Contains(tabla) || ConocimientoIntermediaTables.Contains(tabla))
            {
                return ModuleType.Conocimiento;
            }

            if (CurricularTables.Contains(tabla) || CurricularIntermediaTables.Contains(tabla))
            {
                return ModuleType.Curricular;
            }

            if (ProfesionalTables.Contains(tabla) || ProfesionalIntermediaTables.Contains(tabla))
            {
                return ModuleType.Profesional;
            }

            return ModuleType.Public;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "/";
            }

            path = "/" + path.Trim().Trim('/');
            return path.Equals("//") ? "/" : path;
        }

        private static bool IsIntermediatePath(string path)
        {
            path = NormalizePath(path);
            return InvestigacionIntermediaPaths.Contains(path)
                || ConocimientoIntermediaPaths.Contains(path)
                || CurricularIntermediaPaths.Contains(path)
                || ProfesionalIntermediaPaths.Contains(path);
        }

        private static bool IsIntermediateTable(string tabla)
        {
            return InvestigacionIntermediaTables.Contains(tabla)
                || ConocimientoIntermediaTables.Contains(tabla)
                || CurricularIntermediaTables.Contains(tabla)
                || ProfesionalIntermediaTables.Contains(tabla);
        }

        public record UiPermissions(
            bool CanCreate,
            bool CanEdit,
            bool CanDelete,
            bool CanSave,
            bool CanManageIntermediates)
        {
            public static UiPermissions FullAccess() => new(true, true, true, true, true);
            public static UiPermissions ReadOnly() => new(false, false, false, false, false);
            public static UiPermissions NoAccess() => new(false, false, false, false, false);
        }

        private enum ModuleType
        {
            Public,
            Profesional,
            Investigacion,
            Curricular,
            Conocimiento
        }
    }
}
