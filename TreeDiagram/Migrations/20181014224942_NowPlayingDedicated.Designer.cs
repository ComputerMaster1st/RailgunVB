﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TreeDiagram;

namespace TreeDiagram.Migrations
{
    [DbContext(typeof(TreeDiagramContext))]
    [Migration("20181014224942_NowPlayingDedicated")]
    partial class NowPlayingDedicated
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TreeDiagram.Models.Server.Filter.FilterCaps", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("IncludeBots");

                    b.Property<bool>("IsEnabled");

                    b.Property<int>("Length");

                    b.Property<int>("Percentage");

                    b.HasKey("Id");

                    b.ToTable("FilterCapses");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Filter.FilterUrl", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<List<string>>("BannedUrls");

                    b.Property<bool>("BlockServerInvites");

                    b.Property<bool>("DenyMode");

                    b.Property<bool>("IncludeBots");

                    b.Property<bool>("IsEnabled");

                    b.HasKey("Id");

                    b.ToTable("FilterUrls");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Filter.IgnoredChannels", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("ChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal?>("FilterCapsId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal?>("FilterUrlId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("Id");

                    b.HasIndex("FilterCapsId");

                    b.HasIndex("FilterUrlId");

                    b.ToTable("IgnoredChannels");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Fun.FunBite", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<List<string>>("Bites");

                    b.Property<bool>("IsEnabled");

                    b.HasKey("Id");

                    b.ToTable("FunBites");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Fun.FunRst", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("IsEnabled");

                    b.Property<List<string>>("Rst");

                    b.HasKey("Id");

                    b.ToTable("FunRsts");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerCommand", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("DeleteCmdAfterUse");

                    b.Property<string>("Prefix");

                    b.Property<bool>("RespondToBots");

                    b.HasKey("Id");

                    b.ToTable("ServerCommands");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerJoinLeave", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("ChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<List<string>>("JoinMessages");

                    b.Property<List<string>>("LeaveMessages");

                    b.Property<bool>("SendToDM");

                    b.HasKey("Id");

                    b.ToTable("ServerJoinLeaves");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerMention", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("DisableMentions");

                    b.HasKey("Id");

                    b.ToTable("ServerMentions");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.ServerMusic", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("AutoDownload");

                    b.Property<bool>("AutoSkip");

                    b.Property<decimal>("AutoTextChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("AutoVoiceChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("NowPlayingChannel")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("PlaylistId")
                        .IsRequired();

                    b.Property<bool>("SilentNowPlaying");

                    b.Property<bool>("SilentSongProcessing");

                    b.Property<bool>("VoteSkipEnabled");

                    b.Property<int>("VoteSkipLimit");

                    b.HasKey("Id");

                    b.ToTable("ServerMusics");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Warning.ServerWarning", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<int>("WarnLimit");

                    b.HasKey("Id");

                    b.ToTable("ServerWarnings");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Warning.ServerWarningInfo", b =>
                {
                    b.Property<int>("WarningId")
                        .ValueGeneratedOnAdd();

                    b.Property<List<string>>("Reasons");

                    b.Property<decimal?>("ServerWarningId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("UserId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("WarningId");

                    b.HasIndex("ServerWarningId");

                    b.ToTable("ServerWarningInfo");
                });

            modelBuilder.Entity("TreeDiagram.Models.TreeTimer.TimerRemindMe", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<decimal>("GuildId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Message");

                    b.Property<decimal>("TextChannelId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<DateTime>("TimerExpire");

                    b.Property<decimal>("UserId")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.HasKey("Id");

                    b.ToTable("TimerRemindMes");
                });

            modelBuilder.Entity("TreeDiagram.Models.User.UserCommand", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<string>("Prefix");

                    b.HasKey("Id");

                    b.ToTable("UserCommands");
                });

            modelBuilder.Entity("TreeDiagram.Models.User.UserMention", b =>
                {
                    b.Property<decimal>("Id")
                        .HasConversion(new ValueConverter<decimal, decimal>(v => default(decimal), v => default(decimal), new ConverterMappingHints(precision: 20, scale: 0)));

                    b.Property<bool>("DisableMentions");

                    b.HasKey("Id");

                    b.ToTable("UserMentions");
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Filter.IgnoredChannels", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.Filter.FilterCaps")
                        .WithMany("IgnoredChannels")
                        .HasForeignKey("FilterCapsId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TreeDiagram.Models.Server.Filter.FilterUrl")
                        .WithMany("IgnoredChannels")
                        .HasForeignKey("FilterUrlId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TreeDiagram.Models.Server.Warning.ServerWarningInfo", b =>
                {
                    b.HasOne("TreeDiagram.Models.Server.Warning.ServerWarning")
                        .WithMany("Warnings")
                        .HasForeignKey("ServerWarningId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
