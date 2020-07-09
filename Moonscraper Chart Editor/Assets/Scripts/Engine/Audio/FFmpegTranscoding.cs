#if (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)

/*
 * Copyright (c) 2010 Nicolas George
 * Copyright (c) 2011 Stefano Sabatini
 * Copyright (c) 2014 Andrey Utkin
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

/* This file was translated from C to make use of the FFmpeg.AutoGen library.
 * Original: https://ffmpeg.org/doxygen/4.1/transcode_aac_8c-example.html */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

unsafe class FFmpegTranscoding {

AVFormatContext *ofmt_ctx;
unsafe struct FilteringContext {
    public AVFilterContext *buffersink_ctx;
    public AVFilterContext *buffersrc_ctx;
    public AVFilterGraph *filter_graph;
};
FilteringContext *filter_ctx;

unsafe struct StreamContext {
    public AVCodecContext *dec_ctx;
    public AVCodecContext *enc_ctx;
};
StreamContext *stream_ctx;

/**
 * Open an input file and the required decoder.
 * @param      filename             File to be opened
 * @param[out] input_format_context Format context of opened file
 * @param[out] input_codec_context  Codec context of opened file
 * @return Error code (0 if successful)
 */
int open_input_file(string filename,
                    AVFormatContext **input_format_context,
                    AVCodecContext **input_codec_context)
{
    AVCodecContext *avctx;
    AVCodec *input_codec;
    int error;
    /* Open the input file to read from it. */
    if ((error = avformat_open_input(input_format_context, filename, null,
                                     null)) < 0) {
        Console.WriteLine($"error: Could not open input file '{filename}' (error '{LibAVErrorToString(error)}')");
        *input_format_context = null;
        return error;
    }
    /* Get information on the input file (number of streams etc.). */
    if ((error = avformat_find_stream_info(*input_format_context, null)) < 0) {
        Console.WriteLine($"error: Could not open find stream info (error '{LibAVErrorToString(error)}')");
        avformat_close_input(input_format_context);
        return error;
    }
    /* Make sure that there is only one stream in the input file. */
    if ((*input_format_context)->nb_streams != 1) {
        Console.WriteLine($"error: Expected one audio input stream, but found {(*input_format_context)->nb_streams}");
        avformat_close_input(input_format_context);
        return AVERROR_EXIT;
    }
    /* Find a decoder for the audio stream. */
    if ((input_codec = avcodec_find_decoder((*input_format_context)->streams[0]->codecpar->codec_id)) == null) {
        Console.WriteLine("error: Could not find input codec");
        avformat_close_input(input_format_context);
        return AVERROR_EXIT;
    }
    /* Allocate a new decoding context. */
    avctx = avcodec_alloc_context3(input_codec);
    if (avctx == null) {
        Console.WriteLine("error: Could not allocate a decoding context");
        avformat_close_input(input_format_context);
        return AVERROR(ENOMEM);
    }
    /* Initialize the stream parameters with demuxer information. */
    error = avcodec_parameters_to_context(avctx, (*input_format_context)->streams[0]->codecpar);
    if (error < 0) {
        Console.WriteLine($"error: Could not avcodec_parameters_to_context (error '{LibAVErrorToString(error)}')");
        avformat_close_input(input_format_context);
        avcodec_free_context(&avctx);
        return error;
    }
    /* Open the decoder for the audio stream to use it later. */
    if ((error = avcodec_open2(avctx, input_codec, null)) < 0) {
        Console.WriteLine($"error: Could not open input codec (error '{LibAVErrorToString(error)}')");
        avcodec_free_context(&avctx);
        avformat_close_input(input_format_context);
        return error;
    }
    /* Save the decoder context for easier access later. */
    *input_codec_context = avctx;
    return 0;
}

/**
 * Open an output file and the required encoder.
 * Also set some basic encoder parameters.
 * Some of these parameters are based on the input file's parameters.
 * @param      filename              File to be opened
 * @param      input_codec_context   Codec context of input file
 * @param[out] output_format_context Format context of output file
 * @param[out] output_codec_context  Codec context of output file
 * @return Error code (0 if successful)
 */
int open_output_file(string filename, AVCodecContext *input_codec_context,
                     AVFormatContext **output_format_context, AVCodecContext **output_codec_context)
{
    AVCodecContext *avctx          = null;
    AVIOContext *output_io_context = null;
    AVStream *stream               = null;
    AVCodec *output_codec          = null;
    int error;

    /* Open the output file to write to it. */
    if ((error = avio_open(&output_io_context, filename,
                           AVIO_FLAG_WRITE)) < 0) {
        Console.WriteLine($"error: Could not open output file '{filename}' (error '{LibAVErrorToString(error)}')");
        return error;
    }

    /* Create a new format context for the output container format. */
    if ((*output_format_context = avformat_alloc_context()) == null) {
        Console.WriteLine("error: Could not allocate output format context");
        return AVERROR(ENOMEM);
    }

    /* Associate the output file (pointer) with the container format context. */
    (*output_format_context)->pb = output_io_context;

    /* Guess the desired container format based on the file extension. */
    if (((*output_format_context)->oformat = av_guess_format(null, filename, null)) == null) {
        Console.WriteLine("error: Could not find output file format");
        goto cleanup;
    }

    if (((*output_format_context)->url = av_strdup(filename)) == null) {
        Console.WriteLine("error: Could not allocate url.");
        error = AVERROR(ENOMEM);
        goto cleanup;
    }
    /* Find the encoder to be used by its name. */
    if ((output_codec = avcodec_find_encoder(AVCodecID.AV_CODEC_ID_VORBIS)) == null) {
        Console.WriteLine("error: Could not find a vorbis encoder.");
        goto cleanup;
    }
    /* Create a new audio stream in the output file container. */
    if ((stream = avformat_new_stream(*output_format_context, null)) == null) {
        Console.WriteLine("error: Could not create new stream");
        error = AVERROR(ENOMEM);
        goto cleanup;
    }
    avctx = avcodec_alloc_context3(output_codec);
    if (avctx == null) {
        Console.WriteLine("error: Could not allocate an encoding context");
        error = AVERROR(ENOMEM);
        goto cleanup;
    }

    /* Set the basic encoder parameters.
     * The input file's sample rate is used to avoid a sample rate conversion. */

    /* NOTE: These parameters are tailored for vorbis.
     * See https://ffmpeg.org/ffmpeg-codecs.html#libvorbis
     * Other codecs may need different parameters */
    avctx->channels       = 2;
    avctx->channel_layout = (ulong)av_get_default_channel_layout((int)avctx->channels);
    avctx->sample_rate    = input_codec_context->sample_rate;
    avctx->sample_fmt     = output_codec->sample_fmts[0];
    avctx->global_quality = 7;

    /* Set the sample rate for the container. */
    avctx->time_base.den = input_codec_context->sample_rate;
    avctx->time_base.num = 1;

    /* Some container formats (like MP4) require global headers to be present.
     * Mark the encoder so that it behaves accordingly. */
    if (((*output_format_context)->oformat->flags & AVFMT_GLOBALHEADER) != 0)
        avctx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;

    /* Open the encoder for the audio stream to use it later. */
    if ((error = avcodec_open2(avctx, output_codec, null)) < 0) {
        Console.WriteLine($"error: Could not open output codec (error '{LibAVErrorToString(error)}')");
        goto cleanup;
    }

    error = avcodec_parameters_from_context(stream->codecpar, avctx);
    if (error < 0) {
        Console.WriteLine("error: Could not initialize stream parameters");
        goto cleanup;
    }

    /* Save the encoder context for easier access later. */
    *output_codec_context = avctx;
    return 0;

cleanup:
    avcodec_free_context(&avctx);
    avio_closep(&(*output_format_context)->pb);
    avformat_free_context(*output_format_context);
    *output_format_context = null;
    return error < 0 ? error : AVERROR_EXIT;
}

/**
 * Initialize one data packet for reading or writing.
 * @param packet Packet to be initialized
 */
void init_packet(AVPacket *packet)
{
    av_init_packet(packet);
    /* Set the packet data and size so that it is recognized as being empty. */
    packet->data = null;
    packet->size = 0;
}

/**
 * Initialize one audio frame for reading from the input file.
 * @param[out] frame Frame to be initialized
 * @return Error code (0 if successful)
 */
int init_input_frame(AVFrame **frame)
{
    if ((*frame = av_frame_alloc()) == null) {
        Console.WriteLine("error: Could not allocate input frame");
        return AVERROR(ENOMEM);
    }
    return 0;
}

/**
 * Initialize the audio resampler based on the input and output codec settings.
 * If the input and output sample formats differ, a conversion is required
 * libswresample takes care of this, but requires initialization.
 * @param      input_codec_context  Codec context of the input file
 * @param      output_codec_context Codec context of the output file
 * @param[out] resample_context     Resample context for the required conversion
 * @return Error code (0 if successful)
 */
int init_resampler(AVCodecContext *input_codec_context,
                   AVCodecContext *output_codec_context,
                   SwrContext **resample_context)
{
    int error;

    /*
    * Create a resampler context for the conversion.
    * Set the conversion parameters.
    * Default channel layouts based on the number of channels
    * are assumed for simplicity (they are sometimes not detected
    * properly by the demuxer and/or decoder).
    */
    *resample_context = swr_alloc_set_opts(null,
                                           av_get_default_channel_layout(output_codec_context->channels),
                                           output_codec_context->sample_fmt,
                                           output_codec_context->sample_rate,
                                           av_get_default_channel_layout(input_codec_context->channels),
                                           input_codec_context->sample_fmt,
                                           input_codec_context->sample_rate,
                                           0, null);

    if (*resample_context == null) {
        Console.WriteLine("error: Could not allocate resample context");
        return AVERROR(ENOMEM);
    }

    /*
    * Perform a sanity check so that the number of converted samples is
    * not greater than the number of samples to be converted.
    * If the sample rates differ, this case has to be handled differently
    */
    Debug.Assert(output_codec_context->sample_rate == input_codec_context->sample_rate);

    /* Open the resampler with the specified parameters. */
    if ((error = swr_init(*resample_context)) < 0) {
        Console.WriteLine("error: Could not open resample context");
        swr_free(resample_context);
        return error;
    }

    return 0;
}

/**
 * Initialize a FIFO buffer for the audio samples to be encoded.
 * @param[out] fifo                 Sample buffer
 * @param      output_codec_context Codec context of the output file
 * @return Error code (0 if successful)
 */
int init_fifo(AVAudioFifo **fifo, AVCodecContext *output_codec_context)
{
    /* Create the FIFO buffer based on the specified output sample format. */
    if ((*fifo = av_audio_fifo_alloc(output_codec_context->sample_fmt,
                                     output_codec_context->channels, 1)) == null) {
        Console.WriteLine("error: Could not allocate FIFO");
        return AVERROR(ENOMEM);
    }
    return 0;
}

/**
 * Write the header of the output file container.
 * @param output_format_context Format context of the output file
 * @return Error code (0 if successful)
 */
int write_output_file_header(AVFormatContext *output_format_context)
{
    int error;
    if ((error = avformat_write_header(output_format_context, null)) < 0) {
        Console.WriteLine($"Could not write output file header (error '{LibAVErrorToString(error)}')");
        return error;
    }
    return 0;
}

/**
 * Decode one audio frame from the input file.
 * @param      frame                Audio frame to be decoded
 * @param      input_format_context Format context of the input file
 * @param      input_codec_context  Codec context of the input file
 * @param[out] data_present         Indicates whether data has been decoded
 * @param[out] finished             Indicates whether the end of file has
 *                                  been reached and all data has been
 *                                  decoded. If this flag is false, there
 *                                  is more data to be decoded, i.e., this
 *                                  function has to be called again.
 * @return Error code (0 if successful)
 */
int decode_audio_frame(AVFrame *frame,
                       AVFormatContext *input_format_context,
                       AVCodecContext *input_codec_context,
                       int *data_present, int *finished)
{
    /* Packet used for temporary storage. */
    AVPacket input_packet;
    int error;

    init_packet(&input_packet);

    /* Read one audio frame from the input file into a temporary packet. */
    if ((error = av_read_frame(input_format_context, &input_packet)) < 0) {
        /* If we are at the end of the file, flush the decoder below. */
        if (error == AVERROR_EOF)
            *finished = 1;
        else {
            Console.WriteLine($"error: Could not read frame (error '{LibAVErrorToString(error)}')");
            return error;
        }
    }

    /* Send the audio frame stored in the temporary packet to the decoder.
     * The input audio stream decoder is used to do this. */
    if ((error = avcodec_send_packet(input_codec_context, &input_packet)) < 0) {
        Console.WriteLine($"error: Could not send packet for decoding (error '{LibAVErrorToString(error)}')");
        return error;
    }

    /* Receive one frame from the decoder. */
    error = avcodec_receive_frame(input_codec_context, frame);

    /* If the decoder asks for more data to be able to decode a frame,
     * return indicating that no data is present. */
    if (error == AVERROR(EAGAIN)) {
        error = 0;
        goto cleanup;
    /* If the end of the input file is reached, stop decoding. */
    } else if (error == AVERROR_EOF) {
        *finished = 1;
        error = 0;
        goto cleanup;
    } else if (error < 0) {
        Console.WriteLine($"error: Could not decode frame (error '{LibAVErrorToString(error)}')");
        goto cleanup;
    /* Default case: Return decoded data. */
    } else {
        *data_present = 1;
        goto cleanup;
    }

cleanup:
    av_packet_unref(&input_packet);
    return error;
}

/**
 * Initialize a temporary storage for the specified number of audio samples.
 * The conversion requires temporary storage due to the different format.
 * The number of audio samples to be allocated is specified in frame_size.
 * @param[out] converted_input_samples Array of converted samples. The
 *                                     dimensions are reference, channel
 *                                     (for multi-channel audio), sample.
 * @param      output_codec_context    Codec context of the output file
 * @param      frame_size              Number of samples to be converted in
 *                                     each round
 * @return Error code (0 if successful)
 */
int init_converted_samples(byte ***converted_input_samples,
                           AVCodecContext *output_codec_context,
                           int frame_size)
{
    int error;

    /* Allocate as many pointers as there are audio channels.
     * Each pointer will later point to the audio samples of the corresponding
     * channels (although it may be NULL for interleaved formats).
     */
    if ((*converted_input_samples = (byte**)Marshal.AllocHGlobal(output_codec_context->channels * sizeof(IntPtr))) == null) {
        Console.WriteLine($"error: Could not allocate converted input sample pointers");
        return AVERROR(ENOMEM);
    }

    /* Allocate memory for the samples of all channels in one consecutive
     * block for convenience. */
    if ((error = av_samples_alloc(*converted_input_samples, null,
                                  output_codec_context->channels,
                                  frame_size,
                                  output_codec_context->sample_fmt, 0)) < 0) {
        Console.WriteLine($"error: Could not allocate converted input samples (error '{LibAVErrorToString(error)}')");
        av_freep(&(*converted_input_samples)[0]);
        Marshal.FreeHGlobal((IntPtr)(*converted_input_samples));
        return error;
    }

    return 0;
}

/**
 * Convert the input audio samples into the output sample format.
 * The conversion happens on a per-frame basis, the size of which is
 * specified by frame_size.
 * @param      input_data       Samples to be decoded. The dimensions are
 *                              channel (for multi-channel audio), sample.
 * @param[out] converted_data   Converted samples. The dimensions are channel
 *                              (for multi-channel audio), sample.
 * @param      frame_size       Number of samples to be converted
 * @param      resample_context Resample context for the conversion
 * @return Error code (0 if successful)
 */
int convert_samples(byte **input_data,
                    byte **converted_data, int frame_size,
                    SwrContext *resample_context)
{
    int error;
    /* Convert the samples using the resampler. */
    if ((error = swr_convert(resample_context,
                             converted_data, frame_size,
                             input_data    , frame_size)) < 0) {
        Console.WriteLine($"error: Could not convert input samples (error '{LibAVErrorToString(error)}')");
        return error;
    }
    return 0;
}

/**
 * Add converted input audio samples to the FIFO buffer for later processing.
 * @param fifo                    Buffer to add the samples to
 * @param converted_input_samples Samples to be added. The dimensions are channel
 *                                (for multi-channel audio), sample.
 * @param frame_size              Number of samples to be converted
 * @return Error code (0 if successful)
 */
int add_samples_to_fifo(AVAudioFifo *fifo,
                        byte **converted_input_samples,
                        int frame_size)
{
    int error;

    /* Make the FIFO as large as it needs to be to hold both,
     * the old and the new samples. */
    if ((error = av_audio_fifo_realloc(fifo, av_audio_fifo_size(fifo) + frame_size)) < 0) {
        Console.WriteLine("error: Could not reallocate FIFO");
        return error;
    }

    /* Store the new samples in the FIFO buffer. */
    if (av_audio_fifo_write(fifo, (void **)converted_input_samples,
                            frame_size) < frame_size) {
        Console.WriteLine("error: Could not write data to FIFO");
        return AVERROR_EXIT;
    }

    return 0;
}

/**
 * Read one audio frame from the input file, decode, convert and store
 * it in the FIFO buffer.
 * @param      fifo                 Buffer used for temporary storage
 * @param      input_format_context Format context of the input file
 * @param      input_codec_context  Codec context of the input file
 * @param      output_codec_context Codec context of the output file
 * @param      resampler_context    Resample context for the conversion
 * @param[out] finished             Indicates whether the end of file has
 *                                  been reached and all data has been
 *                                  decoded. If this flag is false,
 *                                  there is more data to be decoded,
 *                                  i.e., this function has to be called
 *                                  again.
 * @return Error code (0 if successful)
 */
int read_decode_convert_and_store(AVAudioFifo *fifo,
                                  AVFormatContext *input_format_context,
                                  AVCodecContext *input_codec_context,
                                  AVCodecContext *output_codec_context,
                                  SwrContext *resampler_context,
                                  int *finished)
{
    /* Temporary storage of the input samples of the frame read from the file. */
    AVFrame *input_frame = null;

    /* Temporary storage for the converted input samples. */
    byte **converted_input_samples = null;
    int data_present = 0;
    int ret = AVERROR_EXIT;

    /* Initialize temporary storage for one input frame. */
    if (init_input_frame(&input_frame) < 0)
        goto cleanup;

    /* Decode one frame worth of audio samples. */
    if (decode_audio_frame(input_frame, input_format_context,
                           input_codec_context, &data_present, finished) < 0)
        goto cleanup;

    /* If we are at the end of the file and there are no more samples
     * in the decoder which are delayed, we are actually finished.
     * This must not be treated as an error. */
    if (*finished != 0) {
        ret = 0;
        goto cleanup;
    }

    /* If there is decoded data, convert and store it. */
    if (data_present != 0) {
        /* Initialize the temporary storage for the converted input samples. */
        if (init_converted_samples(&converted_input_samples, output_codec_context,
                                   input_frame->nb_samples) < 0)
            goto cleanup;

        /* Convert the input samples to the desired output sample format.
         * This requires a temporary storage provided by converted_input_samples. */
        if (convert_samples(input_frame->extended_data, converted_input_samples,
                            input_frame->nb_samples, resampler_context) < 0)
            goto cleanup;

        /* Add the converted input samples to the FIFO buffer for later processing. */
        if (add_samples_to_fifo(fifo, converted_input_samples,
                                input_frame->nb_samples) < 0)
            goto cleanup;

        ret = 0;
    }

    ret = 0;

cleanup:
    if (converted_input_samples != null) {
        av_freep(&converted_input_samples[0]);
        Marshal.FreeHGlobal((IntPtr)converted_input_samples);
    }
    av_frame_free(&input_frame);
    return ret;
}

/**
 * Initialize one input frame for writing to the output file.
 * The frame will be exactly frame_size samples large.
 * @param[out] frame                Frame to be initialized
 * @param      output_codec_context Codec context of the output file
 * @param      frame_size           Size of the frame
 * @return Error code (0 if successful)
 */
int init_output_frame(AVFrame **frame,
                             AVCodecContext *output_codec_context,
                             int frame_size)
{
    int error;
    /* Create a new frame to store the audio samples. */
    if ((*frame = av_frame_alloc()) == null) {
        Console.WriteLine("error: Could not allocate output frame");
        return AVERROR_EXIT;
    }
    /* Set the frame's parameters, especially its size and format.
     * av_frame_get_buffer needs this to allocate memory for the
     * audio samples of the frame.
     * Default channel layouts based on the number of channels
     * are assumed for simplicity. */
    (*frame)->nb_samples     = frame_size;
    (*frame)->channel_layout = output_codec_context->channel_layout;
    (*frame)->format         = (int)output_codec_context->sample_fmt;
    (*frame)->sample_rate    = output_codec_context->sample_rate;
    /* Allocate the samples of the created frame. This call will make
     * sure that the audio frame can hold as many samples as specified. */
    if ((error = av_frame_get_buffer(*frame, 0)) < 0) {
        Console.WriteLine($"error: Could not allocate output frame samples (error '{LibAVErrorToString(error)}')");
        av_frame_free(frame);
        return error;
    }
    return 0;
}

/* Global timestamp for the audio frames. */
Int64 pts = 0;

/**
 * Encode one frame worth of audio to the output file.
 * @param      frame                 Samples to be encoded
 * @param      output_format_context Format context of the output file
 * @param      output_codec_context  Codec context of the output file
 * @param[out] data_present          Indicates whether data has been
 *                                   encoded
 * @return Error code (0 if successful)
 */
int encode_audio_frame(AVFrame *frame,
                       AVFormatContext *output_format_context,
                       AVCodecContext *output_codec_context,
                       int *data_present)
{
    /* Packet used for temporary storage. */
    AVPacket output_packet;
    int error;

    init_packet(&output_packet);

    /* Set a timestamp based on the sample rate for the container. */
    if (frame != null) {
        frame->pts = pts;
        pts += frame->nb_samples;
    }

    /* Send the audio frame stored in the temporary packet to the encoder.
     * The output audio stream encoder is used to do this. */
    error = avcodec_send_frame(output_codec_context, frame);

    /* The encoder signals that it has nothing more to encode. */
    if (error == AVERROR_EOF) {
        error = 0;
        goto cleanup;
    } else if (error < 0) {
        Console.WriteLine($"error: Could not send packet for encoding (error '{LibAVErrorToString(error)}')");
        return error;
    }

    /* Receive one encoded frame from the encoder. */
    error = avcodec_receive_packet(output_codec_context, &output_packet);

    /* If the encoder asks for more data to be able to provide an
     * encoded frame, return indicating that no data is present. */
    if (error == AVERROR(EAGAIN)) {
        error = 0;
        goto cleanup;
    /* If the last frame has been encoded, stop encoding. */
    } else if (error == AVERROR_EOF) {
        error = 0;
        goto cleanup;
    } else if (error < 0) {
        Console.WriteLine($"error: Could not encode frame (error '{LibAVErrorToString(error)}')");
        goto cleanup;
    /* Default case: Return encoded data. */
    } else {
        *data_present = 1;
    }

    /* Write one audio frame from the temporary packet to the output file. */
    if (*data_present != 0 &&
        (error = av_write_frame(output_format_context, &output_packet)) < 0) {
        Console.WriteLine($"error: Could not write frame (error '{LibAVErrorToString(error)}')");
        goto cleanup;
    }

cleanup:
    av_packet_unref(&output_packet);
    return error;
}

/**
 * Load one audio frame from the FIFO buffer, encode and write it to the
 * output file.
 * @param fifo                  Buffer used for temporary storage
 * @param output_format_context Format context of the output file
 * @param output_codec_context  Codec context of the output file
 * @return Error code (0 if successful)
 */
int load_encode_and_write(AVAudioFifo *fifo,
                          AVFormatContext *output_format_context,
                          AVCodecContext *output_codec_context)
{
    /* Temporary storage of the output samples of the frame written to the file. */
    AVFrame *output_frame;

    /* Use the maximum number of possible samples per frame.
     * If there is less than the maximum possible frame size in the FIFO
     * buffer use this number. Otherwise, use the maximum possible frame size. */
    int frame_size = Math.Min(av_audio_fifo_size(fifo),
                              output_codec_context->frame_size);

    int data_written;

    /* Initialize temporary storage for one output frame. */
    if (init_output_frame(&output_frame, output_codec_context, frame_size) < 0)
        return AVERROR_EXIT;

    /* Read as many samples from the FIFO buffer as required to fill the frame.
     * The samples are stored in the frame temporarily. */
    byte*[] temp = output_frame->data;
    fixed (byte** temp2 = temp) {
        if (av_audio_fifo_read(fifo, (void **)temp2, frame_size) < frame_size) {
            Console.WriteLine("error: Could not read data from FIFO");
            av_frame_free(&output_frame);
            return AVERROR_EXIT;
        }
    }

    /* Encode one frame worth of audio samples. */
    if (encode_audio_frame(output_frame, output_format_context,
                           output_codec_context, &data_written) < 0) {
        av_frame_free(&output_frame);
        return AVERROR_EXIT;
    }

    av_frame_free(&output_frame);

    return 0;
}
/**
 * Write the trailer of the output file container.
 * @param output_format_context Format context of the output file
 * @return Error code (0 if successful)
 */
int write_output_file_trailer(AVFormatContext *output_format_context)
{
    int error;

    if ((error = av_write_trailer(output_format_context)) < 0) {
        Console.WriteLine("error: Could not write output file trailer (error '{LibAVErrorToString(error)}')");
        return error;
    }

    return 0;
}

public bool main(string inputFile, string outputFile)
{
    AVFormatContext *input_format_context = null;
    AVFormatContext *output_format_context = null;
    AVCodecContext *input_codec_context = null;
    AVCodecContext *output_codec_context = null;

    SwrContext *resample_context = null;

    AVAudioFifo *fifo = null;

    bool ret = false;

    /* Open the input file for reading. */
    if (open_input_file(inputFile, &input_format_context,
                        &input_codec_context) < 0)
        goto cleanup;

    /* Open the output file for writing. */
    if (open_output_file(outputFile, input_codec_context,
                         &output_format_context, &output_codec_context) < 0)
        goto cleanup;

    /* Initialize the resampler to be able to convert audio sample formats. */
    if (init_resampler(input_codec_context, output_codec_context,
                       &resample_context) < 0)
        goto cleanup;

    /* Initialize the FIFO buffer to store audio samples to be encoded. */
    if (init_fifo(&fifo, output_codec_context) < 0)
        goto cleanup;

    /* Write the header of the output file container. */
    if (write_output_file_header(output_format_context) < 0)
        goto cleanup;

    /* Loop as long as we have input samples to read or output samples
     * to write; abort as soon as we have neither. */
    while (true) {
        /* Use the encoder's desired frame size for processing. */
        int output_frame_size       = output_codec_context->frame_size;
        int finished                = 0;
        /* Make sure that there is one frame worth of samples in the FIFO
         * buffer so that the encoder can do its work.
         * Since the decoder's and the encoder's frame size may differ, we
         * need to FIFO buffer to store as many frames worth of input samples
         * that they make up at least one frame worth of output samples. */
        while (av_audio_fifo_size(fifo) < output_frame_size) {
            /* Decode one frame worth of audio samples, convert it to the
             * output sample format and put it into the FIFO buffer. */
            if (read_decode_convert_and_store(fifo, input_format_context,
                                              input_codec_context,
                                              output_codec_context,
                                              resample_context, &finished) < 0)
                goto cleanup;

            /* If we are at the end of the input file, we continue
             * encoding the remaining audio samples to the output file. */
            if (finished != 0)
                break;
        }

        /* If we have enough samples for the encoder, we encode them.
         * At the end of the file, we pass the remaining samples to
         * the encoder. */
        while (av_audio_fifo_size(fifo) >= output_frame_size ||
               (finished != 0 && av_audio_fifo_size(fifo) > 0))
            /* Take one frame worth of audio samples from the FIFO buffer,
             * encode it and write it to the output file. */
            if (load_encode_and_write(fifo, output_format_context,
                                      output_codec_context) < 0)
                goto cleanup;

        /* If we are at the end of the input file and have encoded
         * all remaining samples, we can exit this loop and finish. */
        if (finished != 0) {
            int data_written;
            /* Flush the encoder as it may have delayed frames. */
            do {
                data_written = 0;
                if (encode_audio_frame(null, output_format_context,
                                       output_codec_context, &data_written) < 0)
                    goto cleanup;
            } while (data_written != 0);
            break;
        }
    }

    /* Write the trailer of the output file container. */
    if (write_output_file_trailer(output_format_context) < 0)
        goto cleanup;

    ret = true;

cleanup:
    if (fifo != null)
        av_audio_fifo_free(fifo);
    swr_free(&resample_context);
    if (output_codec_context != null)
        avcodec_free_context(&output_codec_context);
    if (output_format_context != null) {
        avio_closep(&output_format_context->pb);
        avformat_free_context(output_format_context);
    }
    if (input_codec_context != null)
        avcodec_free_context(&input_codec_context);
    if (input_format_context != null)
        avformat_close_input(&input_format_context);
    return ret;
}

string LibAVErrorToString(int error) {
    var bufferSize = 1024;
    var buffer = stackalloc byte[bufferSize];
    ffmpeg.av_strerror(error, buffer, (ulong) bufferSize);
    var message = Marshal.PtrToStringAnsi((IntPtr) buffer);
    return message;
}

}

#endif
