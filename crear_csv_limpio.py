#!/usr/bin/env python3
"""Script para crear CSV limpio de miembros con datos temporales."""
import csv

# Datos de miembros con información completa
members = [
    ["FullName", "Nombres", "Apellidos", "Cedula", "Email", "Celular", "Direccion", "MemberNumber", "Cargo", "Rango", "Estatus", "FechaIngresoISO"],
    ["Héctor Mario González Henao", "Héctor Mario", "González Henao", "8336963", "hecmarg@yahoo.com", "3104363831", "Calle 53 # 50a-24", "2", "SOCIO", "Full Color", "Activo", "5/1/2013"],
    ["Ramón Antonio González Castaño", "Ramón Antonio", "González Castaño", "15432593", "raangoca@gmail.com", "3137672573", "Calle 51 # 83 96", "5", "SOCIO", "Full Color", "Activo", "5/1/2013"],
    ["César Leonel Rodríguez Galán", "César Leonel", "Rodríguez Galán", "74182011", "ce-galan@hotmail.com", "3192259796", "Carrera 99 A # 48 A-13 Apto 1812", "13", "SOCIO", "Full Color", "Activo", "2/1/2015"],
    ["Jhon Harvey Gómez Patiño", "Jhon Harvey", "Gómez Patiño", "9528949", "jhongo01@hotmail.com", "3006155416", "Circular 1 # 66 B 154", "19", "SOCIO", "Full Color", "Activo", "9/1/2015"],
    ["William Humberto Jiménez Perez", "William Humberto", "Jiménez Perez", "98496540", "williamhjp@hotmail.com", "3017969572", "Calle 32A # 55-33 Int. 301", "30", "SOCIO", "Full Color", "Activo", "6/1/2017"],
    ["Carlos Alberto Araque Betancur", "Carlos Alberto", "Araque Betancur", "71334468", "cocoloquisimo@gmail.com", "3206693638", "Carrera 80 # 41 Sur-31 SADEP", "35", "SOCIO", "Full Color", "Activo", "2/7/2019"],
    ["Milton Darío Gómez Rivera", "Milton Darío", "Gómez Rivera", "98589814", "miltondariog@gmail.com", "3183507127", "Carrera 55 # 58-43", "42", "SOCIO", "Full Color", "Activo", "6/19/2019"],
    ["Carlos Mario Ceballos", "Carlos Mario", "Ceballos", "75049349", "carmace7@gmail.com", "3147244972", "Carrera 60 # 55-56", "47", "SARGENTO DE ARMAS", "Full Color", "Activo", "4/30/2020"],
    ["Carlos Andrés Pérez Areiza", "Carlos Andrés", "Pérez Areiza", "98699136", "carlosap@gmail.com", "3017560517", "Carrera 47 # 19 Sur 136 In 203", "49", "VICEPRESIDENTE", "Full Color", "Activo", "4/30/2020"],
    ["Juan Esteban Suárez Correa", "Juan Esteban", "Suárez Correa", "1095808546", "suarezcorreaj@gmail.com", "3156160015", "Carrera 32A # 77 Sur - 73", "50", "SOCIO", "Full Color", "Activo", "4/30/2020"],
    ["Girlesa María Buitrago", "Girlesa María", "Buitrago", "51983082", "girlesa@gmail.com", "3124739736", "Carrera 72 # 80A -43 Apto 1110 Urbanización la Toscana", "54", "SOCIO", "Full Color", "Activo", "5/26/2021"],
    ["Jhon Emmanuel Arzuza Páez", "Jhon Emmanuel", "Arzuza Páez", "72345562", "jhonarzuza@gmail.com", "3003876340", "Calle 45 AA Sur # 36D-10 Apto 602, Edificio MIrador de las Antillas", "56", "REPORTE RO - SARGENTO DE ARMAS NACIONAL", "Full Color", "Activo", "6/30/2021"],
    ["José Edinson Ospina Cruz", "José Edinson", "Ospina Cruz", "8335981", "chattu.1964@hotmail.com", "3008542336", "Carrera 86 # 48 BB - 19, Medellin", "59", "GERENTE DE NEGOCIOS", "Full Color", "Activo", "10/3/2021"],
    ["Jefferson Montoya Muñoz", "Jefferson", "Montoya Muñoz", "1128406344", "majayura2011@hotmail.com", "3508319246", "Calle 45 # 83-12", "60", "SOCIO", "Full Color", "Activo", "10/3/2021"],
    ["Robinson Alejandro Galvis Parra", "Robinson Alejandro", "Galvis Parra", "71380596", "robin11952@hotmail.com", "3105127314", "Carrera 86 C # 53C 41 Apto 1014", "66", "TESORERO", "Full Color", "Activo", "7/1/2022"],
    ["Carlos Mario Díaz Díaz", "Carlos Mario", "Díaz Díaz", "15506596", "carlosmario.diazdiaz@gmail.com", "3213167406", "Carrera 46D # 48 - 04", "67", "SECRETARIO", "Full Color", "Activo", "8/1/2022"],
    ["Juan Esteban Osorio", "Juan Esteban", "Osorio", "1128399797", "Juan.osorio1429@correo.policia.gov.co", "3112710782", "Calle 50 # 38-12 Apto 801 Barrio Boston Sector Placita de flores", "68", "SOCIO", "Full Color", "Activo", "10/1/2021"],
    ["Carlos Julio Rendón Díaz", "Carlos Julio", "Rendón Díaz", "8162536", "movie.cj@gmail.com", "3507757020", "Avenida 40 Diagonal 51-110, Interior 2222, Torre 1. Unidad Nuevo Milenio. Sector Niquia", "69", "MTO", "Full Color", "Activo", "10/3/2021"],
    ["Daniel Andrey Villamizar Araque", "Daniel Andrey", "Villamizar Araque", "8106002", "dvillamizara@gmail.com", "3106328171", "Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna", "84", "PRESIDENTE", "Full Color", "Activo", "2/1/2024"],
    ["Jhon David Sánchez", "Jhon David", "Sánchez", "1000000072", "jhonda361@gmail.com", "3013424220", "Carrera 26 CC  # 38 A 10 Barrio La Milagrosa", "72", "SOCIO", "Full Color", "Activo", "9/1/2023"],
    ["Ángela Maria Rodríguez Ochoa", "Ángela Maria", "Rodríguez Ochoa", "43703788", "angelarodriguez40350@gmail.com", "3104490476", "Calle 85 # 57-62  Itagui", "46", "SOCIO", "Full Color", "Activo", "11/1/2024"],
    ["Yeferson Bairon Úsuga Agudelo", "Yeferson Bairon", "Úsuga Agudelo", "1000000071", "yeferson915@hotmail.com", "3002891509", "Calle 40 Sur # 75 - 62", "71", "SOCIO", "Full Color", "Activo", "9/1/2023"],
    ["Jennifer Andrea Cardona Benítez", "Jennifer Andrea", "Cardona Benítez", "1035424338", "tucoach21@gmail.com", "3014005382", "Carrera 45 # 47 A 85 Interior 1303 Edificio Vicenza, Barrio Fátima", "", "SOCIO", "Prospecto", "Activo", "1/1/2025"],
    ["Laura Viviana Salazar Moreno", "Laura Viviana", "Salazar Moreno", "1090419626", "laura.s.enf@hotmail.com", "3014307375", "Calle 48F Sur # 40-55 Interior 1308, Urbanización Puerto Luna", "", "SOCIO", "Full Color", "Activo", "6/4/2025"],
    ["José Julián Villamizar Araque", "José Julián", "Villamizar Araque", "8033065", "julianvilllamizar@outlook.com", "3014873771", "", "51", "SOCIO", "Rockets", "Activo", "6/4/2025"],
    ["Gustavo Adolfo Gómez Zuluaga", "Gustavo Adolfo", "Gómez Zuluaga", "1094923731", "gustavo.gomez.temp@fundacionlamamedellin.org", "3132672208", "Carrera 45A # 80 Sur - 75 Apto. 1116, Sabaneta", "", "SOCIO", "Prospecto", "Activo", "10/14/2025"],
    ["Anderson Arlex Betancur Rua", "Anderson Arlex", "Betancur Rua", "1036634452", "armigas7@gmail.com", "3194207889", "Calle 42 Sur # 65 A - 84", "", "SOCIO", "Asociado", "Activo", "10/3/2021"],
    ["Nelson Augusto Montoya Mataute", "Nelson Augusto", "Montoya Mataute", "98472306", "nelson.montoya.temp@fundacionlamamedellin.org", "3137100335", "Carrera 24 A # 59 B - 103", "", "SOCIO", "Prospecto", "Activo", "10/20/2025"],
]

# Escribir CSV con encoding UTF-8 sin BOM
with open("c:/Users/DanielVillamizar/ContabilidadLAMAMedellin/miembros_lama_medellin.csv", "w", encoding="utf-8", newline="") as f:
    writer = csv.writer(f)
    writer.writerows(members)

print("✓ CSV creado: 28 miembros + header (29 líneas)")
