using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLSV.Models;

public partial class HocbasvContext : DbContext
{
    public HocbasvContext()
    {
    }

    public HocbasvContext(DbContextOptions<HocbasvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bomon> Bomons { get; set; }

    public virtual DbSet<CtCtdt> CtCtdts { get; set; }

    public virtual DbSet<CtKhht> CtKhhts { get; set; }

    public virtual DbSet<CtKhhtSv> CtKhhtSvs { get; set; }

    public virtual DbSet<Ctdt> Ctdts { get; set; }

    public virtual DbSet<Giangvien> Giangviens { get; set; }

    public virtual DbSet<Hocky> Hockies { get; set; }

    public virtual DbSet<Khht> Khhts { get; set; }

    public virtual DbSet<Khoa> Khoas { get; set; }

    public virtual DbSet<Kqht> Kqhts { get; set; }

    public virtual DbSet<Lop> Lops { get; set; }

    public virtual DbSet<Monhoc> Monhocs { get; set; }

    public virtual DbSet<Namhoc> Namhocs { get; set; }

    public virtual DbSet<Nganh> Nganhs { get; set; }

    public virtual DbSet<Nhomhp> Nhomhps { get; set; }

    public virtual DbSet<Nienkhoa> Nienkhoas { get; set; }

    public virtual DbSet<Quyen> Quyens { get; set; }
    public virtual DbSet<QuyenGV> QuyenGVs { get; set; }

    public virtual DbSet<Sinhvien> Sinhviens { get; set; }

    public virtual DbSet<Thoigiandangky> Thoigiandangkies { get; set; }

    public virtual DbSet<Xettotnghiep> Xettotnghieps { get; set; }

    public virtual DbSet<XettotnghiepNhom> XettotnghiepNhoms { get; set; }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=HOAITHUY;Initial Catalog=HOCBASV;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bomon>(entity =>
        {
            entity.HasKey(e => e.MaBm).HasName("PK__BOMON__272475AC1CC9CA21");

            entity.ToTable("BOMON");

            entity.Property(e => e.MaBm)
                .HasMaxLength(50)
                .HasColumnName("MaBM");
            entity.Property(e => e.MaKhoa).HasMaxLength(50);
            entity.Property(e => e.TenBm)
                .HasMaxLength(50)
                .HasColumnName("TenBM");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Bomons)
                .HasForeignKey(d => d.MaKhoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOMON__MaKhoa__3E52440B");
        });

        modelBuilder.Entity<CtCtdt>(entity =>
        {
            entity.HasKey(e => new { e.MaCtdt, e.MaMh }).HasName("PK__CT_CTDT__9C3C1D19F2C03236");

            entity.ToTable("CT_CTDT");

            entity.Property(e => e.MaCtdt)
                .HasMaxLength(50)
                .HasColumnName("MaCTDT");
            entity.Property(e => e.MaMh)
                .HasMaxLength(50)
                .HasColumnName("MaMH");
            entity.Property(e => e.LoaiMh).HasColumnName("LoaiMH");
            entity.Property(e => e.MaHk)
                .HasMaxLength(50)
                .HasColumnName("MaHK");
            entity.Property(e => e.MaNh)
                .HasMaxLength(50)
                .HasColumnName("MaNH");
            entity.Property(e => e.MaNhomHp)
                .HasMaxLength(50)
                .HasColumnName("MaNhomHP");

            entity.HasOne(d => d.MaCtdtNavigation).WithMany(p => p.CtCtdts)
                .HasForeignKey(d => d.MaCtdt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_CTDT__MaCTDT__5BE2A6F2");

            entity.HasOne(d => d.MaHkNavigation).WithMany(p => p.CtCtdts)
                .HasForeignKey(d => d.MaHk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_CTDT__MaHK__5DCAEF64");

            entity.HasOne(d => d.MaMhNavigation)       // navigation property đến MonHoc
          .WithMany(p => p.CtCtdts)           // tập hợp CtCtdt trong MonHoc
          .HasForeignKey(d => d.MaMh)         // cột MaMH
          .OnDelete(DeleteBehavior.Cascade)   // <-- bật Cascade ở đây
          .HasConstraintName("FK__CT_CTDT__MaMH__5812160E");


            entity.HasOne(d => d.MaNhNavigation).WithMany(p => p.CtCtdts)
                .HasForeignKey(d => d.MaNh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_CTDT__MaNH__5CD6CB2B");
            entity.HasOne(d => d.MaNhomHpNavigation).WithMany(p => p.CtCtdts)
              .HasForeignKey(d => d.MaNhomHp)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK__CT_CTDT__MaNhomH__5AEE82B9");
        });

        modelBuilder.Entity<CtKhht>(entity =>
        {
            entity.HasKey(e => new { e.MaKhht, e.MaMh }).HasName("PK__CT_KHHT__EF86239CC04352CE");

            entity.ToTable("CT_KHHT");

            entity.Property(e => e.MaKhht)
                .HasMaxLength(50)
                .HasColumnName("MaKHHT");
            entity.Property(e => e.MaMh)
                .HasMaxLength(50)
                .HasColumnName("MaMH");
            entity.Property(e => e.GhiChu).HasMaxLength(50);
            entity.Property(e => e.LoaiMh).HasColumnName("LoaiMH");
            entity.Property(e => e.MaHk)
                .HasMaxLength(50)
                .HasColumnName("MaHK");
            entity.Property(e => e.MaNh)
                .HasMaxLength(50)
                .HasColumnName("MaNH");

            entity.HasOne(d => d.MaHkNavigation).WithMany(p => p.CtKhhts)
                .HasForeignKey(d => d.MaHk)
                .HasConstraintName("FK__CT_KHHT__MaHK__68487DD7");

            entity.HasOne(d => d.MaMhNavigation).WithMany(p => p.CtKhhts)
                .HasForeignKey(d => d.MaMh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_KHHT__MaMH__693CA210");

            entity.HasOne(d => d.MaNhNavigation).WithMany(p => p.CtKhhts)
                .HasForeignKey(d => d.MaNh)
                .HasConstraintName("FK__CT_KHHT__MaNH__6754599E");
        });

        modelBuilder.Entity<CtKhhtSv>(entity =>
        {
            // Khóa chính phức hợp
            entity.HasKey(e => new { e.MaSv, e.MaKhht, e.MaMh })
                  .HasName("PK__CT_KHHT___F9DD6A23CE3170EB");

            entity.ToTable("CT_KHHT_SV");

            // Các cột
            entity.Property(e => e.MaSv)
                .HasColumnName("MaSV")
                .HasMaxLength(50);

            entity.Property(e => e.MaKhht)
                .HasColumnName("MaKHHT")
                .HasMaxLength(50);

            entity.Property(e => e.MaMh)
                .HasColumnName("MaMH")
                .HasMaxLength(50);

            entity.Property(e => e.MaNh)
                .HasColumnName("MaNH")
                .HasMaxLength(50);

            entity.Property(e => e.MaHk)
                .HasColumnName("MaHK")
                .HasMaxLength(50);

            entity.Property(e => e.LoaiMh)
                .HasColumnName("LoaiMH");

            entity.Property(e => e.XacNhanDk)
                .HasColumnName("XacNhanDK");

            // Quan hệ với Hocky
            entity.HasOne(d => d.MaHkNavigation)
                .WithMany(p => p.CtKhhtSvs)
                .HasForeignKey(d => d.MaHk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_KHHT_SV__MaHK__6EF57B66");

            // Quan hệ với Namhoc
            entity.HasOne(d => d.MaNhNavigation)
                .WithMany(p => p.CtKhhtSvs)
                .HasForeignKey(d => d.MaNh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_KHHT_SV__MaNH__6E01572D");

            entity.HasOne(d => d.MaSvNavigation)
                .WithMany(p => p.CtKhhtSvs)
                .HasForeignKey(d => d.MaSv)
                .OnDelete(DeleteBehavior.Cascade)    // <-- bật Cascade Delete ở đây
                .HasConstraintName("FK__CT_KHHT_SV__MaSV__6C190EBB");

            // **Quan hệ với CtKhht**: bật Cascade Delete
            entity.HasOne(d => d.CtKhht)
                .WithMany(p => p.CtKhhtSvs)
                .HasForeignKey(d => new { d.MaKhht, d.MaMh })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__CT_KHHT_SV__6D0D32F4");
        });

    modelBuilder.Entity<Ctdt>(entity =>
        {
            entity.HasKey(e => e.MaCtdt).HasName("PK__CTDT__1E4E40E439CB748F");

            entity.ToTable("CTDT");

            entity.Property(e => e.MaCtdt)
                .HasMaxLength(50)
                .HasColumnName("MaCTDT");
            entity.Property(e => e.HinhThucDt).HasColumnName("HinhThucDT");
            entity.Property(e => e.MaKhoa).HasMaxLength(50);
            entity.Property(e => e.MaNganh).HasMaxLength(50);
            entity.Property(e => e.TenCtdt)
                .HasMaxLength(50)
                .HasColumnName("TenCTDT");
            entity.Property(e => e.TongSoTc).HasColumnName("TongSoTC");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Ctdts)
                .HasForeignKey(d => d.MaKhoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CTDT__MaKhoa__5812160E");

            entity.HasOne(d => d.MaNganhNavigation).WithMany(p => p.Ctdts)
                .HasForeignKey(d => d.MaNganh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CTDT__MaNganh__59063A47");
        });

        modelBuilder.Entity<Giangvien>(entity =>
        {
            entity.HasKey(e => e.MaGv).HasName("PK__GIANGVIE__2725AEF35EEC5110");

            entity.ToTable("GIANGVIEN");

            entity.HasIndex(e => e.Email, "UQ__GIANGVIE__A9D105349D457E35").IsUnique();

            entity.Property(e => e.MaGv)
                .HasMaxLength(50)
                .HasColumnName("MaGV");
            entity.Property(e => e.Anh).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.MaBm)
                .HasMaxLength(50)
                .HasColumnName("MaBM");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.QueQuan).HasMaxLength(50);
            entity.Property(e => e.ResetToken).HasMaxLength(50);
            entity.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .HasColumnName("SDT");
            entity.Property(e => e.TenDn)
                .HasMaxLength(50)
                .HasColumnName("TenDN");

            entity.HasOne(d => d.MaBmNavigation).WithMany(p => p.Giangviens)
                .HasForeignKey(d => d.MaBm)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GIANGVIEN__MaBM__45F365D3");
        });

        modelBuilder.Entity<Hocky>(entity =>
        {
            entity.HasKey(e => e.MaHk).HasName("PK__HOCKY__2725A6E7E1D1FDCF");

            entity.ToTable("HOCKY");

            entity.Property(e => e.MaHk)
                .HasMaxLength(50)
                .HasColumnName("MaHK");
            entity.Property(e => e.TenHk).HasColumnName("TenHK");
        });

        modelBuilder.Entity<Khht>(entity =>
        {
            entity.HasKey(e => e.MaKhht).HasName("PK__KHHT__6DF47E61A44C8825");

            entity.ToTable("KHHT");

            entity.Property(e => e.MaKhht)
                .HasMaxLength(50)
                .HasColumnName("MaKHHT");
            entity.Property(e => e.MaCtdt)
                .HasMaxLength(50)
                .HasColumnName("MaCTDT");
            entity.Property(e => e.MaHk)
                .HasMaxLength(50)
                .HasColumnName("MaHK");
            entity.Property(e => e.MaNh)
                .HasMaxLength(50)
                .HasColumnName("MaNH");
            entity.Property(e => e.MaNk)
                .HasMaxLength(50)
                .HasColumnName("MaNK");
            entity.Property(e => e.SoTcbatBuoc).HasColumnName("SoTCBatBuoc");
            entity.Property(e => e.SoTctong).HasColumnName("SoTCTong");
            entity.Property(e => e.SoTctuChon).HasColumnName("SoTCTuChon");
            entity.Property(e => e.TenKhht)
                .HasMaxLength(50)
                .HasColumnName("TenKHHT");

            entity.HasOne(d => d.MaCtdtNavigation).WithMany(p => p.Khhts)
                .HasForeignKey(d => d.MaCtdt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KHHT__MaCTDT__6383C8BA");

            entity.HasOne(d => d.MaHkNavigation).WithMany(p => p.Khhts)
                .HasForeignKey(d => d.MaHk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KHHT__MaHK__628FA481");

            entity.HasOne(d => d.MaNhNavigation).WithMany(p => p.Khhts)
                .HasForeignKey(d => d.MaNh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KHHT__MaNH__619B8048");

            entity.HasOne(d => d.MaNkNavigation).WithMany(p => p.Khhts)
                 .HasForeignKey(d => d.MaNk)
                 .OnDelete(DeleteBehavior.ClientSetNull)
                 .HasConstraintName("FK__KHHT__MaNK__5DCAEF64");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.MaKhoa).HasName("PK__KHOA__65390405574196F8");

            entity.ToTable("KHOA");

            entity.Property(e => e.MaKhoa).HasMaxLength(50);
            entity.Property(e => e.TenKhoa).HasMaxLength(50);
        });

        modelBuilder.Entity<Kqht>(entity =>
        {
            entity.HasKey(e => e.MaKqht).HasName("PK__KQHT__405D9306BDC1FEE4");

            entity.ToTable("KQHT");

            entity.Property(e => e.MaKqht)
                .HasMaxLength(50)
                .HasColumnName("MaKQHT");
            entity.Property(e => e.DiemCk).HasColumnName("DiemCK");
            entity.Property(e => e.DiemGk).HasColumnName("DiemGK");
            entity.Property(e => e.DiemQt).HasColumnName("DiemQT");
            entity.Property(e => e.DiemTb).HasColumnName("DiemTB");
            entity.Property(e => e.MaHk)
                .HasMaxLength(50)
                .HasColumnName("MaHK");
            entity.Property(e => e.MaMh)
                .HasMaxLength(50)
                .HasColumnName("MaMH");
            entity.Property(e => e.MaNh)
                .HasMaxLength(50)
                .HasColumnName("MaNH");
            entity.Property(e => e.MaSv)
                .HasMaxLength(50)
                .HasColumnName("MaSV");
            entity.Property(e => e.TyLeCk).HasColumnName("TyLeCK");
            entity.Property(e => e.TyLeGk).HasColumnName("TyLeGK");
            entity.Property(e => e.TyLeQt).HasColumnName("TyLeQT");

            entity.HasOne(d => d.MaHkNavigation).WithMany(p => p.Kqhts)
                .HasForeignKey(d => d.MaHk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KQHT__MaHK__72C60C4A");

            entity.HasOne(d => d.MaMhNavigation).WithMany(p => p.Kqhts)
                .HasForeignKey(d => d.MaMh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KQHT__MaMH__70DDC3D8");

            entity.HasOne(d => d.MaNhNavigation).WithMany(p => p.Kqhts)
                .HasForeignKey(d => d.MaNh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KQHT__MaNH__71D1E811");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.Kqhts)
                .HasForeignKey(d => d.MaSv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KQHT__MaSV__6FE99F9F");
        });

        modelBuilder.Entity<Lop>(entity =>
        {
            entity.HasKey(e => e.MaLop).HasName("PK__LOP__3B98D2736EA03D8C");

            entity.ToTable("LOP");

            entity.Property(e => e.MaLop).HasMaxLength(50);
            entity.Property(e => e.MaKhoa).HasMaxLength(50);
            entity.Property(e => e.MaNganh).HasMaxLength(50);
            entity.Property(e => e.TenLop).HasMaxLength(50);

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Lops)
                .HasForeignKey(d => d.MaKhoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LOP__MaKhoa__412EB0B6");

            entity.HasOne(d => d.MaNganhNavigation).WithMany(p => p.Lops)
                .HasForeignKey(d => d.MaNganh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LOP__MaNganh__4222D4EF");
        });

        modelBuilder.Entity<Monhoc>(entity =>
        {
            entity.HasKey(e => e.MaMh).HasName("PK__MONHOC__2725DFD913EF1622");

            entity.ToTable("MONHOC");

            entity.Property(e => e.MaMh)
                .HasMaxLength(50)
                .HasColumnName("MaMH");
            entity.Property(e => e.MaBm)
                .HasMaxLength(50)
                .HasColumnName("MaBM");
            entity.Property(e => e.SoTc).HasColumnName("SoTC");
            entity.Property(e => e.TenMh)
                .HasMaxLength(50)
                .HasColumnName("TenMH");

            entity.HasOne(d => d.MaBmNavigation).WithMany(p => p.Monhocs)
                .HasForeignKey(d => d.MaBm)
                .HasConstraintName("FK__MONHOC__MaBM__48CFD27E");
        });

        modelBuilder.Entity<Namhoc>(entity =>
        {
            entity.HasKey(e => e.MaNh).HasName("PK__NAMHOC__2725D7389C7C493E");

            entity.ToTable("NAMHOC");

            entity.Property(e => e.MaNh)
                .HasMaxLength(50)
                .HasColumnName("MaNH");
            entity.Property(e => e.TenNh)
                .HasMaxLength(50)
                .HasColumnName("TenNH");
        });

        modelBuilder.Entity<Nganh>(entity =>
        {
            entity.HasKey(e => e.MaNganh).HasName("PK__NGANH__A2CEF50DC99067DF");

            entity.ToTable("NGANH");

            entity.Property(e => e.MaNganh).HasMaxLength(50);
            entity.Property(e => e.MaKhoa).HasMaxLength(50);
            entity.Property(e => e.TenNganh).HasMaxLength(50);

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Nganhs)
                .HasForeignKey(d => d.MaKhoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NGANH__MaKhoa__3B75D760");
        });

        modelBuilder.Entity<Nhomhp>(entity =>
        {
            entity.HasKey(e => e.MaNhomHp).HasName("PK__NHOMHP__5A1F5CDB718559AB");

            entity.ToTable("NHOMHP");

            entity.Property(e => e.MaNhomHp)
                .HasMaxLength(50)
                .HasColumnName("MaNhomHP");
            entity.Property(e => e.TenNhomHp)
                .HasMaxLength(200)
                .HasColumnName("TenNhomHP");
        });

        modelBuilder.Entity<Nienkhoa>(entity =>
        {
            entity.HasKey(e => e.MaNk).HasName("PK__NIENKHOA__2725D73FD3C8E66E");

            entity.ToTable("NIENKHOA");

            entity.Property(e => e.MaNk)
                .HasMaxLength(50)
                .HasColumnName("MaNK");
            entity.Property(e => e.TenNk)
                .HasMaxLength(50)
                .HasColumnName("TenNK");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen).HasName("PK__QUYEN__1D4B7ED497A13259");

            entity.ToTable("QUYEN");

            entity.Property(e => e.MaQuyen).HasMaxLength(50);
            entity.Property(e => e.TenQuyen).HasMaxLength(50);

            // ✅ Quan hệ 1-n từ Quyen → QuyenGV
            entity.HasMany(e => e.QuyenGVs)
                .WithOne(qg => qg.Quyen)
                .HasForeignKey(qg => qg.MaQuyen)
                .HasConstraintName("FK_QuyenGV_Quyen")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Sinhvien>(entity =>
        {
            entity.HasKey(e => e.MaSv).HasName("PK__SINHVIEN__2725081A5AF15A84");

            entity.ToTable("SINHVIEN");

            entity.HasIndex(e => e.Email, "UQ__SINHVIEN__A9D10534C2E13C80").IsUnique();

            entity.Property(e => e.MaSv)
                .HasMaxLength(50)
                .HasColumnName("MaSV");
            entity.Property(e => e.Anh).HasMaxLength(50);
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .HasColumnName("CCCD");
            entity.Property(e => e.DanToc).HasMaxLength(20);
            entity.Property(e => e.DiaChi)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.MaCtdt)
                .HasMaxLength(50)
                .HasColumnName("MaCTDT");
            entity.Property(e => e.MaKhoa).HasMaxLength(50);
            entity.Property(e => e.MaLop).HasMaxLength(50);
            entity.Property(e => e.MaNganh).HasMaxLength(50);
            entity.Property(e => e.MaNk)
                .HasMaxLength(50)
                .HasColumnName("MaNK");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.QueQuan).HasMaxLength(50);
            entity.Property(e => e.ResetToken).HasMaxLength(50);
            entity.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .HasColumnName("SDT");
            entity.Property(e => e.TenDn)
                .HasMaxLength(50)
                .HasColumnName("TenDN");

            entity.HasOne(d => d.MaCtdtNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaCtdt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SINHVIEN__MaCTDT__6B24EA82");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaKhoa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SINHVIEN__MaKhoa__6754599E");

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SINHVIEN__MaLop__693CA210");

            entity.HasOne(d => d.MaNganhNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaNganh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SINHVIEN__MaNgan__68487DD7");

            entity.HasOne(d => d.MaNkNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaNk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SINHVIEN__MaNK__6A30C649");
        });

        modelBuilder.Entity<Thoigiandangky>(entity =>
        {
            entity.HasKey(e => new { e.MaNh, e.MaHk }).HasName("PK__THOIGIAN__45578D565235B2DF");

            entity.ToTable("THOIGIANDANGKY");

            entity.Property(e => e.MaNh)
                .HasMaxLength(50)
                .HasColumnName("MaNH");
            entity.Property(e => e.MaHk)
                .HasMaxLength(50)
                .HasColumnName("MaHK");
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");

            entity.HasOne(d => d.MaHkNavigation).WithMany(p => p.Thoigiandangkies)
                .HasForeignKey(d => d.MaHk)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__THOIGIANDA__MaHK__73BA3083");

            entity.HasOne(d => d.MaNhNavigation).WithMany(p => p.Thoigiandangkies)
                .HasForeignKey(d => d.MaNh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__THOIGIANDA__MaNH__72C60C4A");
        });

        modelBuilder.Entity<Xettotnghiep>(entity =>
        {
            entity.HasKey(e => e.MaCtdt)
                .HasName("PK__XETTOTNG__1E4E40E430822D07");

            entity.ToTable("XETTOTNGHIEP");

            entity.Property(e => e.MaCtdt)
                .HasMaxLength(50)
                .HasColumnName("MaCTDT");

            entity.Property(e => e.GpatoiThieu)
                .HasColumnType("decimal(3,2)")
                .HasColumnName("GPAToiThieu");

            entity.Property(e => e.SoTcBatBuoc)
                .HasColumnName("SoTC_BatBuoc");

            entity.Property(e => e.SoTcTong)
                .HasColumnName("SoTCTong");

            entity.Property(e => e.SoTcTuChon)
                .HasColumnName("SoTC_TuChon");

            // --- Cấu hình quan hệ 1-1 với CTDT ---
            entity.HasOne(d => d.MaCtdtNavigation)       // navigation từ Xettotnghiep sang CTDT
                .WithOne(p => p.Xettotnghiep)            // navigation ngược từ CTDT sang Xettotnghiep
                .HasForeignKey<Xettotnghiep>(d => d.MaCtdt)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_XETTOTNGHIEP_CTDT");
        });

        modelBuilder.Entity<XettotnghiepNhom>(entity =>
        {
            entity.HasKey(e => new { e.MaCtdt, e.MaNhomHp }).HasName("PK__XETTOTNG__BBEFB5293CDFA203");

            entity.ToTable("XETTOTNGHIEP_NHOM");

            entity.Property(e => e.MaCtdt)
                .HasMaxLength(50)
                .HasColumnName("MaCTDT");
            entity.Property(e => e.MaNhomHp)
                .HasMaxLength(50)
                .HasColumnName("MaNhomHP");

            entity.HasOne(d => d.MaCtdtNavigation).WithMany(p => p.XettotnghiepNhoms)
                .HasForeignKey(d => d.MaCtdt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__XETTOTNGH__MaCTD__02FC7413");

            entity.HasOne(d => d.MaNhomHpNavigation).WithMany(p => p.XettotnghiepNhoms)
                .HasForeignKey(d => d.MaNhomHp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__XETTOTNGH__MaNho__03F0984C");
        });
        modelBuilder.Entity<QuyenGV>(entity =>
        {
            entity.HasKey(e => new { e.MaQuyen, e.MaGv }).HasName("PK_QuyenGV");

            entity.ToTable("QuyenGV");

            entity.Property(e => e.MaQuyen).HasMaxLength(50);
            entity.Property(e => e.MaGv).HasMaxLength(50).HasColumnName("MaGV");

            entity.HasOne(d => d.Quyen)
                .WithMany(p => p.QuyenGVs)
                .HasForeignKey(d => d.MaQuyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuyenGV_Quyen");

            entity.HasOne(d => d.Giangvien)
                .WithMany(p => p.QuyenGVs)
                .HasForeignKey(d => d.MaGv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuyenGV_Giangvien");
        });


        OnModelCreatingPartial(modelBuilder);
    }



    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
