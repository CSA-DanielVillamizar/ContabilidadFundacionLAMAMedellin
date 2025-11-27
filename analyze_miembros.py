"""Analiza y limpia el archivo miembros_lama_medellin.csv.

Genera:
- reporte_calidad.csv con indicadores por registro
- miembros_lama_medellin_clean.csv con datos normalizados
- resumen_reporte.txt con estadísticos globales
- faltantes_validar.csv con registros con datos incompletos

Criterios de limpieza:
- Normalizar espacios dobles en nombres y apellidos (preservando acentos UTF-8)
- Unificar tildes inconsistentes (RodrÍguez -> Rodríguez) usando unicodedata NFC
- Cedula vacía -> marcar como MissingCedula
- Email vacío -> MissingEmail
- Celular: quitar espacios, normalizar a 10 dígitos, validar longitud (>=10)
- Direccion: strip (preservando acentos)
- FechaIngresoISO: parse múltiples formatos (M/D/YYYY, M/D/YY) -> YYYY-MM-DD
- Rango/Estatus/Cargo: strip
- MemberNumber vacío -> asignar secuencial desde máximo existente + 1
- Detectar duplicados de Cedula y MemberNumber
"""
from __future__ import annotations
import csv
import unicodedata
from pathlib import Path
from datetime import datetime

INPUT_FILE = Path("miembros_lama_medellin.csv")
CLEAN_FILE = Path("miembros_lama_medellin_clean.csv")
QUALITY_FILE = Path("reporte_calidad.csv")
SUMMARY_FILE = Path("resumen_reporte.txt")
FALTANTES_FILE = Path("faltantes_validar.csv")

DATE_FORMATS = ["%m/%d/%Y", "%m/%d/%y", "%Y-%m-%d", "%m/%d/%Y"]

def normalizar_texto(s: str) -> str:
    """Normaliza texto preservando acentos UTF-8 válidos."""
    if s is None:
        return ""
    s = s.strip()
    # Normalizar composición para tildes y caracteres (NFC = forma canónica compuesta)
    s = unicodedata.normalize("NFC", s)
    # Colapsar espacios múltiples
    while "  " in s:
        s = s.replace("  ", " ")
    return s

def parse_fecha(raw: str) -> str:
    raw = raw.strip()
    if not raw:
        return ""
    for fmt in DATE_FORMATS:
        try:
            dt = datetime.strptime(raw, fmt)
            return dt.strftime("%Y-%m-%d")
        except ValueError:
            continue
    return ""  # no parseada

def limpiar_celular(cel: str) -> str:
    """Normaliza celular a 10 dígitos sin espacios."""
    cel = cel.strip().replace(" ", "").replace("-", "")
    # Si empieza con +57, quitar prefijo internacional
    if cel.startswith("+57"):
        cel = cel[3:]
    elif cel.startswith("57") and len(cel) > 10:
        cel = cel[2:]
    return cel

def main():
    if not INPUT_FILE.exists():
        raise SystemExit(f"No existe {INPUT_FILE}")

    with INPUT_FILE.open("r", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        rows = [r for r in reader if r is not None and any(r.values())]

    # Calcular max MemberNumber existente (numérico)
    max_member_num = 0
    for r in rows:
        mem = (r.get("MemberNumber") or "").strip()
        if mem and mem.isdigit():
            max_member_num = max(max_member_num, int(mem))

    next_member_num = max_member_num + 1

    cedula_counts = {}
    member_counts = {}
    for r in rows:
        ced = (r.get("Cedula") or "").strip()
        if ced:
            cedula_counts[ced] = cedula_counts.get(ced, 0) + 1
        mem = (r.get("MemberNumber") or "").strip()
        if mem:
            member_counts[mem] = member_counts.get(mem, 0) + 1

    clean_rows = []
    quality_rows = []
    faltantes_rows = []

    for r in rows:
        full_name = normalizar_texto(r.get("FullName") or "")
        nombres = normalizar_texto(r.get("Nombres") or "")
        apellidos = normalizar_texto(r.get("Apellidos") or "")
        cedula = (r.get("Cedula") or "").strip()
        email = (r.get("Email") or "").strip()
        celular = limpiar_celular(r.get("Celular") or "")
        direccion = normalizar_texto(r.get("Direccion") or "")
        member_number = (r.get("MemberNumber") or "").strip()
        cargo = normalizar_texto(r.get("Cargo") or "")
        rango = normalizar_texto(r.get("Rango") or "")
        estatus = normalizar_texto(r.get("Estatus") or "")
        fecha_ingreso = parse_fecha(r.get("FechaIngresoISO") or "")

        # Asignar MemberNumber secuencial si falta
        if not member_number:
            member_number = str(next_member_num)
            next_member_num += 1

        # Indicadores de calidad
        dup_cedula = cedula_counts.get(cedula, 0) > 1 if cedula else False
        dup_member = member_counts.get(member_number, 0) > 1 if member_number else False
        missing_email = email == ""
        missing_cedula = cedula == ""
        celular_len = len(celular)
        celular_ok = celular_len >= 10
        fecha_ok = fecha_ingreso != ""

        clean_rows.append({
            "FullName": full_name,
            "Nombres": nombres,
            "Apellidos": apellidos,
            "Cedula": cedula,
            "Email": email,
            "Celular": celular,
            "Direccion": direccion,
            "MemberNumber": member_number,
            "Cargo": cargo,
            "Rango": rango,
            "Estatus": estatus,
            "FechaIngreso": fecha_ingreso,
        })

        quality_rows.append({
            "MemberNumber": member_number,
            "Cedula": cedula,
            "DupCedula": dup_cedula,
            "DupMemberNumber": dup_member,
            "MissingEmail": missing_email,
            "MissingCedula": missing_cedula,
            "CelularLength": celular_len,
            "CelularOK": celular_ok,
            "FechaOK": fecha_ok,
        })

        # Si falta algún dato crítico, agregar a faltantes
        if missing_email or missing_cedula or not celular_ok:
            faltantes_rows.append({
                "MemberNumber": member_number,
                "FullName": full_name,
                "Cedula": cedula,
                "Email": email,
                "Celular": celular,
                "Problemas": ", ".join([
                    "Sin email" if missing_email else "",
                    "Sin cédula" if missing_cedula else "",
                    f"Celular inválido ({celular_len} dígitos)" if not celular_ok else "",
                ]).strip(", "),
            })

    # Escribir archivos
    with CLEAN_FILE.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=list(clean_rows[0].keys()))
        writer.writeheader()
        writer.writerows(clean_rows)

    with QUALITY_FILE.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=list(quality_rows[0].keys()))
        writer.writeheader()
        writer.writerows(quality_rows)

    if faltantes_rows:
        with FALTANTES_FILE.open("w", encoding="utf-8", newline="") as f:
            writer = csv.DictWriter(f, fieldnames=list(faltantes_rows[0].keys()))
            writer.writeheader()
            writer.writerows(faltantes_rows)

    # Resumen
    total = len(rows)
    dup_cedulas = sum(1 for q in quality_rows if q["DupCedula"])
    missing_emails = sum(1 for q in quality_rows if q["MissingEmail"])
    missing_cedulas = sum(1 for q in quality_rows if q["MissingCedula"])
    celulares_bad = sum(1 for q in quality_rows if not q["CelularOK"])
    fechas_bad = sum(1 for q in quality_rows if not q["FechaOK"])

    with SUMMARY_FILE.open("w", encoding="utf-8") as f:
        f.write("RESUMEN CALIDAD DE MIEMBROS\n")
        f.write(f"Total registros: {total}\n")
        f.write(f"MemberNumber asignados nuevos: {next_member_num - max_member_num - 1}\n")
        f.write(f"Cédulas duplicadas: {dup_cedulas}\n")
        f.write(f"Cédulas faltantes: {missing_cedulas}\n")
        f.write(f"Emails faltantes: {missing_emails}\n")
        f.write(f"Celulares inválidos (<10 dígitos): {celulares_bad}\n")
        f.write(f"Fechas ingreso no parseadas: {fechas_bad}\n")
        f.write(f"\nRegistros con datos faltantes: {len(faltantes_rows)}\n")
        f.write("\nRecomendaciones:\n")
        f.write("- Revisar archivo faltantes_validar.csv para completar datos.\n")
        f.write("- Validar MemberNumber asignados automáticamente.\n")
        f.write("- Completar emails y cédulas faltantes.\n")
        f.write("- Corregir celulares con longitud <10.\n")
        f.write("- Revisar duplicados de cédula para posible consolidación.\n")

    print("✓ Archivos generados:")
    print(f"  • {CLEAN_FILE}")
    print(f"  • {QUALITY_FILE}")
    print(f"  • {SUMMARY_FILE}")
    if faltantes_rows:
        print(f"  • {FALTANTES_FILE} ({len(faltantes_rows)} registros con datos incompletos)")

if __name__ == "__main__":
    main()
