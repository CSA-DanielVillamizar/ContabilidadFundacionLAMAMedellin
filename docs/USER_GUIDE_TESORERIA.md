# Guía de Usuario - Módulo de Tesorería
**Sistema de Contabilidad LAMA Medellín**  
**Versión:** 1.0.0  
**Última actualización:** 21 de enero de 2026

---

## 📖 Tabla de Contenido
1. [Introducción](#introducción)
2. [Acceso al Módulo](#acceso-al-módulo)
3. [Cuentas Financieras](#cuentas-financieras)
4. [Movimientos de Tesorería](#movimientos-de-tesorería)
5. [Aportes Mensuales](#aportes-mensuales)
6. [Preguntas Frecuentes](#preguntas-frecuentes)

---

## 🎯 Introducción

El **Módulo de Tesorería** permite gestionar todas las operaciones financieras de LAMA Medellín de manera centralizada, incluyendo:
- Cuentas bancarias y cajas
- Ingresos y egresos con clasificación y aprobación
- Aportes mensuales de miembros
- Validación de saldos y períodos contables cerrados

**Roles con acceso:**
- **Admin:** Acceso completo (crear, editar, aprobar, cerrar períodos)
- **Tesorero:** Acceso completo a operaciones diarias
- **Junta/Consulta:** Solo lectura (próxima versión)

---

## 🔐 Acceso al Módulo

### **Navegación**
1. Inicia sesión en el sistema
2. En el menú lateral izquierdo, localiza la sección **TESORERÍA**
3. Verás las siguientes opciones:
   - **Cuentas Financieras**
   - **Movimientos Tesorería**
   - **Aportes Mensuales**
   - (Más opciones: Recibos, Egresos, Deudores, etc.)

**Nota:** Si no ves estas opciones, verifica que tu usuario tenga rol de Admin o Tesorero.

---

## 🏦 Cuentas Financieras

### **¿Qué son?**
Las cuentas financieras representan las cuentas bancarias y cajas que utiliza LAMA Medellín para manejar sus fondos.

### **Acceso**
- Menú → **TESORERÍA** → **Cuentas Financieras**

### **Funcionalidades**

#### **Ver Cuentas**
Al ingresar a la página, verás una tabla con:
- **Código:** Identificador único (ej: BANCO-BCOL-001)
- **Nombre:** Descripción de la cuenta (ej: Bancolombia - Cuenta Principal Tesorería)
- **Banco:** Entidad bancaria
- **Número:** Número de cuenta enmascarado (ej: ****5678)
- **Saldo:** Saldo actual en COP
- **Activa:** Estado (Sí/No)

#### **Crear Nueva Cuenta**
1. Haz clic en el botón **"Nueva Cuenta"**
2. Completa el formulario:
   - **Código:** Identificador único (requerido)
   - **Nombre:** Nombre descriptivo (requerido)
   - **Banco:** Nombre del banco (ej: Bancolombia, Davivienda)
   - **Número de Cuenta (enmascarado):** Solo últimos 4 dígitos (ej: ****5678)
   - **Tipo:** Selecciona Bancaria o Caja
3. Haz clic en **"Guardar"**

**Validaciones:**
- El código debe ser único
- No se permiten códigos duplicados

#### **Cuenta Bancolombia (Seed)**
Al instalar el sistema, automáticamente se crea la cuenta principal:
- **Código:** BANCO-BCOL-001
- **Nombre:** Bancolombia - Cuenta Principal Tesorería
- **Tipo:** Bancaria
- **Estado:** Activa
- **Saldo Inicial:** $0

---

## 💰 Movimientos de Tesorería

### **¿Qué son?**
Los movimientos de tesorería son todos los ingresos y egresos que afectan las cuentas financieras de LAMA Medellín.

### **Acceso**
- Menú → **TESORERÍA** → **Movimientos Tesorería**

### **Estados de un Movimiento**
- **Borrador:** Movimiento creado pero no confirmado; no afecta saldos
- **Aprobado:** Movimiento confirmado; afecta el saldo de la cuenta
- **Anulado:** Movimiento cancelado; no afecta saldos

**⚠️ Regla importante:** Solo los movimientos **Aprobados** afectan los saldos calculados.

### **Funcionalidades**

#### **Ver Movimientos**
La página muestra una tabla con los últimos 200 movimientos, ordenados por fecha descendente:
- **Número:** Identificador único (ej: MV-2025-A1B2C3)
- **Fecha:** Fecha del movimiento
- **Cuenta:** Cuenta financiera asociada
- **Tipo:** Ingreso o Egreso
- **Estado:** Borrador/Aprobado/Anulado
- **Valor:** Monto en COP
- **Descripción:** Detalle del movimiento

#### **Filtros Disponibles**
- **Fecha Inicio/Fin:** Rango de fechas
- **Cuenta:** Filtrar por cuenta específica
- **Tipo:** Ingreso o Egreso
- **Estado:** Borrador/Aprobado/Anulado

Haz clic en **"Filtrar"** después de seleccionar tus criterios.

#### **Crear Nuevo Movimiento**
1. Haz clic en **"Nuevo Movimiento"**
2. Completa el formulario:
   - **Número:** Se genera automáticamente (ej: MV-2025-XXXXXX)
   - **Fecha:** Fecha del movimiento
   - **Cuenta:** Selecciona la cuenta financiera
   - **Tipo:** Ingreso o Egreso
   - **Fuente Ingreso** (si es Ingreso): Selecciona la clasificación (ej: Aporte Mensual, Donación, Venta Merchandising)
   - **Categoría Egreso** (si es Egreso): Selecciona la clasificación (ej: Ayudas Sociales, Compra Insumos, Papelería)
   - **Medio:** Forma de pago (Transferencia, Consignación, Efectivo, Nequi, Daviplata, Tarjeta, Cheque)
   - **Valor:** Monto en COP (sin puntos ni símbolos)
   - **Descripción:** Detalle del movimiento
3. Haz clic en **"Guardar"**

**Validaciones:**
- No se permite crear movimientos en períodos cerrados (meses con cierre contable)
- El número de movimiento debe ser único
- Debe seleccionarse Fuente de Ingreso para Ingresos o Categoría de Egreso para Egresos

#### **Medios de Pago Disponibles**
- **Transferencia:** Transferencia bancaria electrónica
- **Consignación:** Depósito en banco
- **Efectivo:** Pago en efectivo
- **Nequi:** Pago por Nequi
- **Daviplata:** Pago por Daviplata
- **Tarjeta:** Pago con tarjeta débito/crédito
- **Cheque:** Pago con cheque

### **Clasificación de Movimientos**

#### **Fuentes de Ingreso**
- **Aporte Mensual Miembro** (APORTE-MEN): $20,000 COP mensuales por miembro activo
- **Venta Merchandising** (VENTA-MERCH): Venta de productos promocionales
- **Venta Club Arte** (VENTA-CLUB-ART): Ingresos del club de arte
- **Venta Club Café** (VENTA-CLUB-CAFE): Ingresos del club de café
- **Venta Club Cerveza** (VENTA-CLUB-CERV): Ingresos del club de cerveza
- **Venta Club Comida** (VENTA-CLUB-COMI): Ingresos del club de comida
- **Donaciones** (DONACION): Donaciones recibidas
- **Eventos** (EVENTO): Ingresos por eventos y actividades
- **Renovación Membresía** (RENOVACION-MEM): Renovaciones anuales
- **Otros Ingresos** (OTROS): Cualquier otro ingreso no clasificado

#### **Categorías de Egreso**
- **Ayudas Sociales** (AYUDA-SOCIAL): Ayudas a miembros o comunidad
- **Logística de Eventos** (EVENTO-LOG): Gastos de organización de eventos
- **Compra Merchandising** (COMPRA-MERCH): Compra de productos para venta
- **Compra Insumos Café** (COMPRA-CLUB-CAFE): Insumos para club de café
- **Compra Insumos Cerveza** (COMPRA-CLUB-CERV): Insumos para club de cerveza
- **Compra Insumos Comida** (COMPRA-CLUB-COMI): Insumos para club de comida
- **Compra Otros Insumos** (COMPRA-CLUB-OTROS): Otros insumos para clubes
- **Papelería y Útiles** (ADMIN-PAPEL): Material de oficina
- **Transporte** (ADMIN-TRANSP): Transporte y desplazamientos
- **Servicios** (ADMIN-SERVICIOS): Servicios públicos y administrativos
- **Mantenimiento** (MANTENIMIENTO): Reparaciones y mantenimiento
- **Otros Gastos** (OTROS-GASTOS): Cualquier otro gasto no clasificado

---

## 💳 Aportes Mensuales

### **¿Qué son?**
Los aportes mensuales son las contribuciones regulares de $20,000 COP que cada miembro activo de LAMA Medellín realiza mensualmente.

### **Acceso**
- Menú → **TESORERÍA** → **Aportes Mensuales**

### **Estados de un Aporte**
- **Pendiente:** Aún no se ha registrado el pago
- **Pagado:** El aporte fue recibido y registrado
- **Exonerado:** El miembro está exonerado de pago en ese período

### **Funcionalidades**

#### **Ver Aportes**
La página muestra una tabla con los aportes del mes actual por defecto:
- **Miembro:** Nombre completo y número de socio
- **Año:** Año del aporte
- **Mes:** Mes del aporte (1=Enero, 12=Diciembre)
- **Valor:** Monto esperado ($20,000 COP)
- **Estado:** Pendiente/Pagado/Exonerado
- **Fecha Pago:** Fecha en que se registró el pago (si aplica)

#### **Filtros Disponibles**
- **Año:** Selecciona el año a consultar
- **Mes:** Selecciona el mes (1-12)
- **Estado:** Pendiente/Pagado/Exonerado
- **Miembro:** Busca un miembro específico

Haz clic en **"Filtrar"** después de seleccionar tus criterios.

#### **Reglas Importantes**
- **Un aporte por miembro/mes/año:** No se permiten duplicados
- **Vinculación con Movimientos:** Al registrar el pago, se puede vincular con un MovimientoTesoreria de tipo Ingreso
- **Valor Estándar:** El sistema usa $20,000 COP como valor esperado por defecto

---

## ❓ Preguntas Frecuentes

### **1. ¿Puedo editar un movimiento Aprobado?**
No directamente. Los movimientos aprobados no deben modificarse para mantener la integridad contable. Si necesitas corregir un error, debes:
1. Anular el movimiento incorrecto
2. Crear un nuevo movimiento con la información correcta

### **2. ¿Qué pasa si intento crear un movimiento en un mes cerrado?**
El sistema te impedirá crear o modificar movimientos en períodos que ya tienen cierre contable. Contacta al administrador si necesitas ajustar un período cerrado (requiere reabrir el cierre).

### **3. ¿Cómo se calcula el saldo de una cuenta?**
```
Saldo Actual = Saldo Inicial + Ingresos Aprobados - Egresos Aprobados
```
Solo los movimientos en estado **Aprobado** afectan el saldo.

### **4. ¿Puedo tener varias cuentas bancarias?**
Sí, el sistema soporta múltiples cuentas. Cada movimiento debe estar asociado a una cuenta específica.

### **5. ¿Qué hago si un miembro paga varios meses juntos?**
Debes crear:
1. Un MovimientoTesoreria de Ingreso por el valor total
2. Registros de AporteMensual para cada mes cubierto (estado: Pagado)

### **6. ¿Cómo sé si un aporte mensual está pendiente?**
Ingresa a **Aportes Mensuales**, filtra por el mes/año deseado y estado "Pendiente". Verás la lista de miembros con aportes no pagados.

### **7. ¿Puedo clasificar un movimiento en ambas categorías (Ingreso y Egreso)?**
No. Cada movimiento debe ser exclusivamente un Ingreso o un Egreso, no ambos. Selecciona el tipo correcto al crear el movimiento.

### **8. ¿Qué significan los códigos de las cuentas (ej: BANCO-BCOL-001)?**
Son identificadores únicos que siguen este formato:
- **BANCO-**: Cuenta bancaria
- **CAJA-**: Caja (efectivo)
- **BCOL**: Iniciales del banco (Bancolombia)
- **001**: Número secuencial

### **9. ¿Puedo desactivar una cuenta financiera?**
Sí, pero no puedes eliminarla si tiene movimientos asociados. Al desactivar una cuenta, deja de aparecer en los listados activos pero se mantiene el histórico.

### **10. ¿Dónde veo el histórico completo de una cuenta?**
En **Movimientos Tesorería**, filtra por la cuenta específica y ajusta el rango de fechas para ver todos los movimientos históricos.

---

## 🆘 Soporte

Si tienes dudas o encuentras problemas:
1. Revisa esta guía de usuario
2. Contacta al Administrador del Sistema
3. Reporta errores técnicos al equipo de desarrollo

---

**Fin de la Guía de Usuario - Tesorería v1.0.0**
