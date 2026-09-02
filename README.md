Document Management System

A full-stack document management system built for government, law firm or any business that use paperwork since they still use lot of manual documents and can be lost or something happening to it, supporting document upload, versioning, category organization, status workflows, and audit logging.

Overview

This system allows users to upload, organize, and manage documents through their lifecycle — from Draft to Active to Archived — with a full version history and audit trail for compliance purposes.

Tech Stack

Backend

ASP.NET Core Web API (.NET 8)
Entity Framework Core (SQL Server / LocalDB)
ASP.NET Core Identity (cookie-based authentication, role-based authorization)
Swagger / OpenAPI

Frontend

React (Vite)
React Router
react-pdf (PDF preview)
Plain CSS
Features
Authentication & Authorization — register/login/logout via ASP.NET Identity
Document Management — create, view, and delete documents, organized into categories and sub-categories
Version Control — upload new versions of a document without overwriting previous ones; full version history per document
Status Workflow — enforced transitions between Draft → Active → Archived, validated server-side
Categories — hierarchical categories with document counts, expandable to show documents inside each one
Search & Filter — filter documents by title and category
PDF Preview — preview uploaded PDF documents in-browser without downloading
Audit Trail — every document action (create, upload, status change) is logged, viewable by Admin/ComplianceOfficer roles
File Download — download any version of an uploaded document





Only these transitions are permitted; all changes are validated server-side and logged to the audit trail.

Author

Shudufhadzo Tshimuka — Software Developer Intern, SITA
